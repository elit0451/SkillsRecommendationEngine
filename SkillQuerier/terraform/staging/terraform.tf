
terraform {
 backend "s3" {
 encrypt = true
 bucket = "terraform-skills-recommendation-engine-s3-storage"
 region = "eu-west-1"
 key = "terraform_sq_stg.tfstate"
 }
}