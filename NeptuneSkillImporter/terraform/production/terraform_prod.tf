# Region
provider "aws" {
  region = "eu-west-1"
}


# VPC related
resource "aws_vpc" "prod-skill-importer-vpc" {
  cidr_block           = "10.0.0.0/16"
  enable_dns_hostnames = true

  tags = {
    Name        = "prod-skill-importer-vpc",
    Environment = "production"
  }
}

resource "aws_subnet" "prod-skill-importer-subnet" {
  vpc_id            = aws_vpc.prod-skill-importer-vpc.id
  cidr_block        = "10.0.1.0/24"
  availability_zone = "eu-west-1a"

  tags = {
    Name        = "prod-skill-importer-subnet",
    Environment = "production"
  }
}

resource "aws_subnet" "prod-skill-importer-subnet-bk" {
  vpc_id            = aws_vpc.prod-skill-importer-vpc.id
  cidr_block        = "10.0.2.0/24"
  availability_zone = "eu-west-1b"

  tags = {
    Name        = "prod-skill-importer-subnet-bk",
    Environment = "production"
  }
}

resource "aws_internet_gateway" "prod-skill-importer-internet-gateway" {
  vpc_id = aws_vpc.prod-skill-importer-vpc.id

  tags = {
    Name        = "prod-skill-importer-internet-gateway",
    Environment = "production"
  }
}

resource "aws_route_table" "prod-skill-importer-route-table" {
  vpc_id = aws_vpc.prod-skill-importer-vpc.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.prod-skill-importer-internet-gateway.id
  }

  tags = {
    Name        = "prod-skill-importer-route-table",
    Environment = "production"
  }
}

/*
resource "aws_vpc_endpoint" "prod-skill-importer-sqs-endpoint" {
  vpc_id              = aws_vpc.prod-skill-importer-vpc.id
  service_name        = "com.amazonaws.eu-west-1.sqs"
  subnet_ids          = ["${aws_subnet.prod-skill-importer-subnet.id}"]
  security_group_ids  = ["${aws_security_group.prod-skill-importer-security-group.id}"]
  vpc_endpoint_type   = "Interface"
  private_dns_enabled = true

  tags = {
    Name        = "prod-skill-importer-sqs-endpoint",
    Environment = "production"
  }
}

*/

resource "aws_vpc_endpoint" "prod-skill-importer-s3-endpoint" {
  vpc_id          = aws_vpc.prod-skill-importer-vpc.id
  service_name    = "com.amazonaws.eu-west-1.s3"
  route_table_ids = ["${aws_route_table.prod-skill-importer-route-table.id}"]

  tags = {
    Name        = "prod-skill-importer-s3-endpoint",
    Environment = "production"
  }
}

resource "aws_neptune_subnet_group" "prod-skill-importer-subnet-group" {
  name       = "prod-skill-importer-subnet-group"
  subnet_ids = ["${aws_subnet.prod-skill-importer-subnet.id}", "${aws_subnet.prod-skill-importer-subnet-bk.id}"]

  tags = {
    Name        = "prod-skill-importer-subnet-group",
    Environment = "production"
  }
}
resource "aws_security_group" "prod-skill-importer-security-group" {
  name        = "prod-skill-importer-security-group"
  description = "Secure all job post related traffic"
  vpc_id      = aws_vpc.prod-skill-importer-vpc.id

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

# Availability zones
data "aws_availability_zones" "prod-availability-zones" {
  state = "available"
}

# Neptune
resource "aws_neptune_cluster" "prod-skill-importer-cluster" {
  engine                    = "neptune"
  backup_retention_period   = 1
  apply_immediately         = true
  availability_zones        = data.aws_availability_zones.prod-availability-zones.names
  vpc_security_group_ids    = ["${aws_security_group.prod-skill-importer-security-group.id}"]
  neptune_subnet_group_name = aws_neptune_subnet_group.prod-skill-importer-subnet-group.name
  skip_final_snapshot       = true
}

resource "aws_neptune_cluster_instance" "prod-skill-importer-instance" {
  count              = 1
  cluster_identifier = aws_neptune_cluster.prod-skill-importer-cluster.id
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

resource "aws_instance" "prod-skill-importer-ec2-instance" {
  ami                         = data.aws_ami.amazon-linux-2-ami.id
  instance_type               = "t2.micro"
  vpc_security_group_ids      = ["${aws_security_group.prod-skill-importer-security-group.id}"]
  subnet_id                   = aws_subnet.prod-skill-importer-subnet.id
  key_name                    = "ssh-ec2-test"
  associate_public_ip_address = true
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
dotnet restore
dotnet run ${aws_neptune_cluster.prod-skill-importer-cluster.endpoint} 8182
shutdown -h now
--//
  EOT

  tags = {
    Name        = "prod-SkillImporterEC2"
    Environment = "production"
  }
}

# Cloudwatch
resource "aws_cloudwatch_event_rule" "prod-skill-importer-start-rule" {
  name                = "prod-skill-importer-start-rule"
  schedule_expression = "rate(1 day)"
}

resource "aws_cloudwatch_event_target" "prod-skill-importer-start-target" {
  rule = aws_cloudwatch_event_rule.prod-skill-importer-start-rule.name
  arn  = aws_lambda_function.prod-skill-importer-starter-lambda.arn
}

resource "aws_lambda_permission" "allow-cloudwatch-to-call-skill-importer-starter" {
  statement_id  = "AllowExecutionFromCloudWatch"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.prod-skill-importer-starter-lambda.function_name
  principal     = "events.amazonaws.com"
  source_arn    = aws_cloudwatch_event_rule.prod-skill-importer-start-rule.arn
}

# Lambda
resource "aws_lambda_function" "prod-skill-importer-starter-lambda" {
  function_name    = "prod-SkillImporterStarter"
  handler          = "SkillImporterManager::SkillImporterManager.Function::FunctionHandler"
  runtime          = "dotnetcore2.1"
  role             = "arn:aws:iam::833191605868:role/DeleteThisRole"
  filename         = "../../src/data/SkillImporterMANAGER.zip"
  source_code_hash = filebase64sha256("../../src/data/SkillImporterMANAGER.zip")
  timeout          = 10
  memory_size      = 256

  tags = {
    Name        = "prod-SkillImporterStarter"
    Environment = "production"
  }

  environment {
    variables = {
      AWS_EC2_INSTANCE = aws_instance.prod-skill-importer-ec2-instance.id
    }
  }
}
