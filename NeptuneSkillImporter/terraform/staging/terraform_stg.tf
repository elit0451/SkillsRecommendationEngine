# Region
provider "aws" {
  region = "eu-west-1"
}


# VPC related
resource "aws_vpc" "stg-skill-importer-vpc" {
  cidr_block           = "10.0.0.0/16"
  enable_dns_hostnames = true

  tags = {
    Name        = "stg-skill-importer-vpc",
    Environment = "staging"
  }
}

resource "aws_subnet" "stg-skill-importer-subnet" {
  vpc_id            = aws_vpc.stg-skill-importer-vpc.id
  cidr_block        = "10.0.1.0/24"
  availability_zone = "eu-west-1a"

  tags = {
    Name        = "stg-skill-importer-subnet",
    Environment = "staging"
  }
}

resource "aws_subnet" "stg-skill-importer-subnet-bk" {
  vpc_id            = aws_vpc.stg-skill-importer-vpc.id
  cidr_block        = "10.0.2.0/24"
  availability_zone = "eu-west-1b"

  tags = {
    Name        = "stg-skill-importer-subnet-bk",
    Environment = "staging"
  }
}

resource "aws_internet_gateway" "stg-skill-importer-internet-gateway" {
  vpc_id = aws_vpc.stg-skill-importer-vpc.id

  tags = {
    Name        = "stg-skill-importer-internet-gateway",
    Environment = "staging"
  }
}

resource "aws_route_table" "stg-skill-importer-route-table" {
  vpc_id = aws_vpc.stg-skill-importer-vpc.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.stg-skill-importer-internet-gateway.id
  }

  tags = {
    Name        = "stg-skill-importer-route-table",
    Environment = "staging"
  }
}

resource "aws_main_route_table_association" "stg-main-route-association" {
  vpc_id         = aws_vpc.stg-skill-importer-vpc.id
  route_table_id = aws_route_table.stg-skill-importer-route-table.id
}

/*
resource "aws_vpc_endpoint" "stg-skill-importer-sqs-endpoint" {
  vpc_id              = aws_vpc.stg-skill-importer-vpc.id
  service_name        = "com.amazonaws.eu-west-1.sqs"
  subnet_ids          = ["${aws_subnet.stg-skill-importer-subnet.id}"]
  security_group_ids  = ["${aws_security_group.stg-skill-importer-security-group.id}"]
  vpc_endpoint_type   = "Interface"
  private_dns_enabled = true

  tags = {
    Name        = "stg-skill-importer-sqs-endpoint",
    Environment = "staging"
  }
}

*/

resource "aws_vpc_endpoint" "stg-skill-importer-s3-endpoint" {
  vpc_id          = aws_vpc.stg-skill-importer-vpc.id
  service_name    = "com.amazonaws.eu-west-1.s3"
  route_table_ids = ["${aws_route_table.stg-skill-importer-route-table.id}"]

  tags = {
    Name        = "stg-skill-importer-s3-endpoint",
    Environment = "staging"
  }
}

resource "aws_neptune_subnet_group" "stg-skill-importer-subnet-group" {
  name       = "stg-skill-importer-subnet-group"
  subnet_ids = ["${aws_subnet.stg-skill-importer-subnet.id}", "${aws_subnet.stg-skill-importer-subnet-bk.id}"]

  tags = {
    Name        = "stg-skill-importer-subnet-group",
    Environment = "staging"
  }
}
resource "aws_security_group" "stg-skill-importer-security-group" {
  name        = "stg-skill-importer-security-group"
  description = "Secure all job post related traffic"
  vpc_id      = aws_vpc.stg-skill-importer-vpc.id

  ingress {
    from_port   = 22
    to_port     = 22
    protocol    = "TCP"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    from_port = 8182
    to_port   = 8182
    protocol  = "TCP"
    self      = true
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# Neptune
resource "aws_neptune_cluster" "stg-skill-importer-cluster" {
  engine                    = "neptune"
  backup_retention_period   = 1
  apply_immediately         = true
  availability_zones        = ["eu-west-1a", "eu-west-1b", "eu-west-1c"]
  vpc_security_group_ids    = ["${aws_security_group.stg-skill-importer-security-group.id}"]
  neptune_subnet_group_name = aws_neptune_subnet_group.stg-skill-importer-subnet-group.name
  skip_final_snapshot       = true
}

resource "aws_neptune_cluster_instance" "stg-skill-importer-instance" {
  count              = 1
  cluster_identifier = aws_neptune_cluster.stg-skill-importer-cluster.id
  engine             = "neptune"
  instance_class     = "db.t3.medium"
  apply_immediately  = true
}

# EC2
data "aws_ami" "amazon-linux-2-ami" {
  most_recent = true
  owners      = ["amazon"]

  filter {
    name   = "owner-alias"
    values = ["amazon"]
  }

  filter {
    name   = "name"
    values = ["amzn2-ami-hvm*"]
  }
}

resource "aws_instance" "stg-skill-importer-ec2-instance" {
  ami                         = data.aws_ami.amazon-linux-2-ami.id
  instance_type               = "t2.micro"
  vpc_security_group_ids      = ["${aws_security_group.stg-skill-importer-security-group.id}"]
  subnet_id                   = aws_subnet.stg-skill-importer-subnet.id
  key_name                    = "ssh-ec2-test"
  associate_public_ip_address = true
  iam_instance_profile        = "CloudWatchMetricsAccess"
  user_data                   = <<-EOT
Content-Type: multipart/mixed; boundary="//"
MIME-Version: 1.0

--//
Content-Type: text/cloud-config; charset="us-ascii"
MIME-Version: 1.0
Content-Transfer-Encoding: 7bit
Content-Disposition: attachment; filename="cloud-config.txt"

#cloud-config
cloud_final_modules:
- [scripts-user, always]

--//
Content-Type: text/x-shellscript; charset="us-ascii"
MIME-Version: 1.0
Content-Transfer-Encoding: 7bit
Content-Disposition: attachment; filename="userdata.txt"

#!/bin/bash
export DOTNET_CLI_HOME=/home/ec2-user
rm -rf SkillsRecommendationEngine
sudo rpm -Uvh https://packages.microsoft.com/config/rhel/7/packages-microsoft-prod.rpm
sudo yum update -y
sudo yum install git -y
sudo yum install dotnet-sdk-2.1 -y
git clone https://github.com/elit0451/SkillsRecommendationEngine.git
cd SkillsRecommendationEngine/NeptuneSkillImporter/src/
git checkout staging
dotnet restore
dotnet run ${aws_neptune_cluster.prod-skill-importer-cluster.endpoint} 8182
shutdown -h now
--//
  EOT

  tags = {
    Name        = "stg-SkillImporterEC2"
    Environment = "staging"
  }
}

# Cloudwatch
resource "aws_cloudwatch_event_rule" "stg-skill-importer-start-rule" {
  name                = "stg-skill-importer-start-rule"
  schedule_expression = "rate(1 day)"
}

resource "aws_cloudwatch_event_target" "stg-skill-importer-start-target" {
  rule = aws_cloudwatch_event_rule.stg-skill-importer-start-rule.name
  arn  = aws_lambda_function.stg-skill-importer-starter-lambda.arn
}

resource "aws_lambda_permission" "allow-cloudwatch-to-call-skill-importer-starter" {
  statement_id  = "AllowExecutionFromCloudWatch"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.stg-skill-importer-starter-lambda.function_name
  principal     = "events.amazonaws.com"
  source_arn    = aws_cloudwatch_event_rule.stg-skill-importer-start-rule.arn
}

# Lambda
resource "aws_lambda_function" "stg-skill-importer-starter-lambda" {
  function_name    = "stg-SkillImporterStarter"
  handler          = "SkillImporterManager::SkillImporterManager.Function::FunctionHandler"
  runtime          = "dotnetcore2.1"
  role             = "arn:aws:iam::833191605868:role/DeleteThisRole"
  filename         = "../../src/data/SkillImporterMANAGER.zip"
  source_code_hash = filebase64sha256("../../src/data/SkillImporterMANAGER.zip")
  timeout          = 10
  memory_size      = 256

  tags = {
    Name        = "stg-SkillImporterStarter"
    Environment = "staging"
  }

  environment {
    variables = {
      AWS_EC2_INSTANCE = aws_instance.stg-skill-importer-ec2-instance.id
    }
  }
}
