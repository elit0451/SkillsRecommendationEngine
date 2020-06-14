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

  filter {
    name   = "owner-alias"
    values = ["amazon"]
  }

  filter {
    name   = "name"
    values = ["amzn2-ami-hvm*"]
  }
}
