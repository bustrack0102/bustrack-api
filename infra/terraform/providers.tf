terraform {
  required_version = ">= 1.6.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }

  backend "azurerm" {}
}

provider "azurerm" {
  resource_provider_registrations = "none"

  features {
    cognitive_account {
      purge_soft_delete_on_destroy = true
    }
  }
}
