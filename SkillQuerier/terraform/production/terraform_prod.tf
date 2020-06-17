# Region
provider "aws" {
  region = "eu-west-1"
}

# VPC
data "aws_subnet" "prod-skill-importer-subnet" {
  id = "subnet-03362d02f2d1cadaf"
}

data "aws_subnet" "prod-skill-importer-subnet-bk" {
  id = "subnet-09b672555093d86fb"
}

data "aws_security_group" "prod-skill-importer-security-group" {
  id = "sg-009be18c58f9e5952"
}

# Lambda
resource "aws_lambda_function" "prod-SkillQuerier-lambda" {
  function_name    = "prod-SkillQuerier"
  handler          = "SkillQuerier::SkillQuerier.Functions::Get"
  runtime          = "dotnetcore2.1"
  role             = "arn:aws:iam::833191605868:role/DeleteThisRole"
  filename         = "../../src/SkillQuerier/bin/Release/netcoreapp2.1/SkillQuerier.zip"
  source_code_hash = filebase64sha256("../../src/SkillQuerier/bin/Release/netcoreapp2.1/SkillQuerier.zip")
  timeout          = 10
  memory_size      = 512

  vpc_config {
    subnet_ids         = ["${data.aws_subnet.prod-skill-importer-subnet.id}", "${data.aws_subnet.prod-skill-importer-subnet-bk.id}"]
    security_group_ids = ["${data.aws_security_group.prod-skill-importer-security-group.id}"]
  }

  tags = {
    Name        = "prod-SkillQuerier"
    Environment = "production"
  }

  environment {
    variables = {
      NEPTUNE_ENDPOINT = "tf-20200617093124839400000001.cluster-cjpaettbkbiu.eu-west-1.neptune.amazonaws.com"
    }
  }
}

# API Gateway
resource "aws_api_gateway_rest_api" "prod-SkillQuerier-api" {
  name = "prod-SkillQuerier-api"
}

resource "aws_api_gateway_resource" "prod-SkillQuerier-resource" {
  path_part   = "relatedskills"
  parent_id   = aws_api_gateway_rest_api.prod-SkillQuerier-api.root_resource_id
  rest_api_id = aws_api_gateway_rest_api.prod-SkillQuerier-api.id
}

resource "aws_api_gateway_method" "prod-SkillQuerier-post-method" {
  rest_api_id   = aws_api_gateway_rest_api.prod-SkillQuerier-api.id
  resource_id   = aws_api_gateway_resource.prod-SkillQuerier-resource.id
  http_method   = "POST"
  authorization = "NONE"
}

resource "aws_api_gateway_method" "prod-SkillQuerier-options-method" {
  rest_api_id   = aws_api_gateway_rest_api.prod-SkillQuerier-api.id
  resource_id   = aws_api_gateway_resource.prod-SkillQuerier-resource.id
  http_method   = "OPTIONS"
  authorization = "NONE"
}

resource "aws_api_gateway_integration" "prod-SkillQuerier-api-integration-post" {
  rest_api_id             = aws_api_gateway_rest_api.prod-SkillQuerier-api.id
  resource_id             = aws_api_gateway_resource.prod-SkillQuerier-resource.id
  http_method             = aws_api_gateway_method.prod-SkillQuerier-post-method.http_method
  integration_http_method = "ANY"
  type                    = "AWS_PROXY"
  uri                     = aws_lambda_function.prod-SkillQuerier-lambda.invoke_arn

  depends_on = [aws_lambda_function.prod-SkillQuerier-lambda, aws_api_gateway_method_response.prod-SkillQuerier-post-response]
}

resource "aws_api_gateway_integration" "prod-SkillQuerier-api-integration-options" {
  rest_api_id = aws_api_gateway_rest_api.prod-SkillQuerier-api.id
  resource_id = aws_api_gateway_resource.prod-SkillQuerier-resource.id
  http_method = aws_api_gateway_method.prod-SkillQuerier-options-method.http_method
  type        = "MOCK"

  depends_on = [aws_api_gateway_method_response.prod-SkillQuerier-options-response]
}

resource "aws_api_gateway_method_response" "prod-SkillQuerier-options-response" {
  rest_api_id = aws_api_gateway_rest_api.prod-SkillQuerier-api.id
  resource_id = aws_api_gateway_resource.prod-SkillQuerier-resource.id
  http_method = aws_api_gateway_method.prod-SkillQuerier-options-method.http_method
  status_code = "200"
  response_models = {
    "application/json" = "Empty"
  }
  response_parameters = {
    "method.response.header.Access-Control-Allow-Headers" = true,
    "method.response.header.Access-Control-Allow-Methods" = true,
    "method.response.header.Access-Control-Allow-Origin"  = true
  }

  depends_on = [aws_api_gateway_method.prod-SkillQuerier-options-method]
}

resource "aws_api_gateway_method_response" "prod-SkillQuerier-post-response" {
  rest_api_id = aws_api_gateway_rest_api.prod-SkillQuerier-api.id
  resource_id = aws_api_gateway_resource.prod-SkillQuerier-resource.id
  http_method = aws_api_gateway_method.prod-SkillQuerier-post-method.http_method
  status_code = "200"
  response_models = {
    "application/json" = "Empty"
  }
  response_parameters = {
    "method.response.header.Access-Control-Allow-Origin" = true
  }

  depends_on = [aws_api_gateway_method.prod-SkillQuerier-post-method]
}

resource "aws_api_gateway_deployment" "prod-SkillQuerier-api-deployment" {
  depends_on = [aws_api_gateway_integration.prod-SkillQuerier-api-integration-post, aws_api_gateway_integration.prod-SkillQuerier-api-integration-options]

  rest_api_id = aws_api_gateway_rest_api.prod-SkillQuerier-api.id
  stage_name  = "production"
}

resource "aws_lambda_permission" "prod-SkillQuerier-allow-invoke" {
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.prod-SkillQuerier-lambda.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_api_gateway_rest_api.prod-SkillQuerier-api.execution_arn}/*/*/relatedskills"
}
