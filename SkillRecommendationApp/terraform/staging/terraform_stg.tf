# Region
provider "aws" {
  region = "eu-west-1"
}

# Lambda
resource "aws_lambda_function" "stg-SkillRecommendationApp-lambda" {
  function_name    = "stg-SkillRecommendationApp"
  handler          = "SkillRecommendationApp::SkillRecommendationApp.Function::Get"
  runtime          = "dotnetcore2.1"
  role             = "arn:aws:iam::833191605868:role/DeleteThisRole"
  filename         = "../../src/SkillRecommendationApp/bin/Debug/netcoreapp2.1/SkillRecommendationApp.zip"
  source_code_hash = filebase64sha256("../../src/SkillRecommendationApp/bin/Debug/netcoreapp2.1/SkillRecommendationApp.zip")
  timeout          = 10
  memory_size      = 256

  tags = {
    Name        = "stg-SkillRecommendationApp"
    Environment = "staging"
  }
}


# API Gateway
resource "aws_api_gateway_rest_api" "skill-recommendation-api" {
  name = "skillRecommendationAppApi-stg"
}

resource "aws_api_gateway_resource" "recommendation-receiver-resource" {
  path_part   = "recommendationReceiver"
  parent_id   = aws_api_gateway_rest_api.skill-recommendation-api.root_resource_id
  rest_api_id = aws_api_gateway_rest_api.skill-recommendation-api.id
}

resource "aws_api_gateway_method" "recommendation-receiver-any" {
  rest_api_id   = aws_api_gateway_rest_api.skill-recommendation-api.id
  resource_id   = aws_api_gateway_resource.recommendation-receiver-resource.id
  http_method   = "ANY"
  authorization = "NONE"
}

resource "aws_api_gateway_integration" "integration" {
  rest_api_id             = aws_api_gateway_rest_api.skill-recommendation-api.id
  resource_id             = aws_api_gateway_resource.recommendation-receiver-resource.id
  http_method             = aws_api_gateway_method.recommendation-receiver-any.http_method
  integration_http_method = "ANY"
  type                    = "AWS_PROXY"
  uri                     = aws_lambda_function.stg-SkillRecommendationApp-lambda.invoke_arn
}

resource "aws_api_gateway_deployment" "recommendation-receiver-api-deployment" {
  depends_on = [aws_api_gateway_integration.integration]

  rest_api_id = aws_api_gateway_rest_api.skill-recommendation-api.id
  stage_name  = "staging"
}