# Region
provider "aws" {
  region = "eu-west-1"
}

# VPC
data "aws_subnet" "stg-skill-importer-subnet" {
  id = "subnet-05a5e49ada2d10f31"
}

data "aws_subnet" "stg-skill-importer-subnet-bk" {
  id = "subnet-04ea7a7be5f7b7640"
}

data "aws_security_group" "stg-skill-importer-security-group" {
  id = "sg-095570e6c37f63818"
}

# Lambda
resource "aws_lambda_function" "stg-SkillQuerier-lambda" {
  function_name    = "stg-SkillQuerier"
  handler          = "SkillQuerier::SkillQuerier.Functions::Get"
  runtime          = "dotnetcore2.1"
  role             = "arn:aws:iam::833191605868:role/DeleteThisRole"
  filename         = "../../src/SkillQuerier/bin/Debug/netcoreapp2.1/SkillQuerier.zip"
  source_code_hash = filebase64sha256("../../src/SkillQuerier/bin/Debug/netcoreapp2.1/SkillQuerier.zip")
  timeout          = 10
  memory_size      = 512

  vpc_config {
    subnet_ids         = ["${data.aws_subnet.stg-skill-importer-subnet.id}", "${data.aws_subnet.stg-skill-importer-subnet-bk.id}"]
    security_group_ids = ["${data.aws_security_group.stg-skill-importer-security-group.id}"]
  }

  tags = {
    Name        = "stg-SkillQuerier"
    Environment = "staging"
  }

  environment {
    variables = {
      NEPTUNE_ENDPOINT = "tf-20200617093124839400000001.cluster-cjpaettbkbiu.eu-west-1.neptune.amazonaws.com"
    }
  }
}

# API Gateway
resource "aws_api_gateway_rest_api" "stg-SkillQuerier-api" {
  name = "stg-SkillQuerier-api"
}

resource "aws_api_gateway_resource" "stg-SkillQuerier-resource" {
  path_part   = "relatedskills"
  parent_id   = aws_api_gateway_rest_api.stg-SkillQuerier-api.root_resource_id
  rest_api_id = aws_api_gateway_rest_api.stg-SkillQuerier-api.id
}

resource "aws_api_gateway_method" "stg-SkillQuerier-post-method" {
  rest_api_id   = aws_api_gateway_rest_api.stg-SkillQuerier-api.id
  resource_id   = aws_api_gateway_resource.stg-SkillQuerier-resource.id
  http_method   = "POST"
  authorization = "NONE"
}

resource "aws_api_gateway_method" "stg-SkillQuerier-options-method" {
  rest_api_id   = aws_api_gateway_rest_api.stg-SkillQuerier-api.id
  resource_id   = aws_api_gateway_resource.stg-SkillQuerier-resource.id
  http_method   = "OPTIONS"
  authorization = "NONE"
}

resource "aws_api_gateway_integration" "stg-SkillQuerier-api-integration-post" {
  rest_api_id             = aws_api_gateway_rest_api.stg-SkillQuerier-api.id
  resource_id             = aws_api_gateway_resource.stg-SkillQuerier-resource.id
  http_method             = aws_api_gateway_method.stg-SkillQuerier-post-method.http_method
  integration_http_method = "ANY"
  type                    = "AWS_PROXY"
  uri                     = aws_lambda_function.stg-SkillQuerier-lambda.invoke_arn

  depends_on = [aws_lambda_function.stg-SkillQuerier-lambda, aws_api_gateway_method_response.stg-SkillQuerier-post-response]
}

resource "aws_api_gateway_integration" "stg-SkillQuerier-api-integration-options" {
  rest_api_id = aws_api_gateway_rest_api.stg-SkillQuerier-api.id
  resource_id = aws_api_gateway_resource.stg-SkillQuerier-resource.id
  http_method = aws_api_gateway_method.stg-SkillQuerier-options-method.http_method
  type        = "MOCK"

  depends_on = [aws_api_gateway_method_response.stg-SkillQuerier-options-response]
}

resource "aws_api_gateway_method_response" "stg-SkillQuerier-options-response" {
  rest_api_id = aws_api_gateway_rest_api.stg-SkillQuerier-api.id
  resource_id = aws_api_gateway_resource.stg-SkillQuerier-resource.id
  http_method = aws_api_gateway_method.stg-SkillQuerier-options-method.http_method
  status_code = "200"
  response_models = {
    "application/json" = "Empty"
  }
  response_parameters = {
    "method.response.header.Access-Control-Allow-Headers" = true,
    "method.response.header.Access-Control-Allow-Methods" = true,
    "method.response.header.Access-Control-Allow-Origin"  = true
  }

  depends_on = [aws_api_gateway_method.stg-SkillQuerier-options-method]
}

resource "aws_api_gateway_method_response" "stg-SkillQuerier-post-response" {
  rest_api_id = aws_api_gateway_rest_api.stg-SkillQuerier-api.id
  resource_id = aws_api_gateway_resource.stg-SkillQuerier-resource.id
  http_method = aws_api_gateway_method.stg-SkillQuerier-post-method.http_method
  status_code = "200"
  response_models = {
    "application/json" = "Empty"
  }
  response_parameters = {
    "method.response.header.Access-Control-Allow-Origin" = true
  }

  depends_on = [aws_api_gateway_method.stg-SkillQuerier-post-method]
}

resource "aws_api_gateway_deployment" "stg-SkillQuerier-api-deployment" {
  depends_on = [aws_api_gateway_integration.stg-SkillQuerier-api-integration-post, aws_api_gateway_integration.stg-SkillQuerier-api-integration-options]

  rest_api_id = aws_api_gateway_rest_api.stg-SkillQuerier-api.id
  stage_name  = "staging"
}

resource "aws_lambda_permission" "stg-SkillQuerier-allow-invoke" {
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.stg-SkillQuerier-lambda.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_api_gateway_rest_api.stg-SkillQuerier-api.execution_arn}/*/*/relatedskills"
}
