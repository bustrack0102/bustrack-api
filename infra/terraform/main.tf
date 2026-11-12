data "azurerm_resource_group" "main" {
  name = var.resource_group_name
}

data "azurerm_container_registry" "acr" {
  name                = var.acr_name
  resource_group_name = data.azurerm_resource_group.main.name
}

locals {
  tags = {
    project     = "bustrack"
    environment = "production"
    managed_by  = "terraform"
    university  = "uttt"
  }
  acr_server = data.azurerm_container_registry.acr.login_server
  acr_user   = data.azurerm_container_registry.acr.admin_username
  acr_pass   = data.azurerm_container_registry.acr.admin_password
}

resource "azurerm_log_analytics_workspace" "main" {
  name                = "law-bustrack-ncus"
  location            = var.location
  resource_group_name = data.azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
  tags                = local.tags
}

resource "azurerm_application_insights" "main" {
  name                = "appi-bustrack-ncus"
  location            = var.location
  resource_group_name = data.azurerm_resource_group.main.name
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "web"
  tags                = local.tags
}

resource "azurerm_storage_account" "media" {
  name                            = var.storage_account_name
  resource_group_name             = data.azurerm_resource_group.main.name
  location                        = var.location
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  min_tls_version                 = "TLS1_2"
  https_traffic_only_enabled      = true
  allow_nested_items_to_be_public = true
  tags                            = local.tags
}

resource "azurerm_storage_container" "media_cdn" {
  name                  = "media"
  storage_account_id    = azurerm_storage_account.media.id
  container_access_type = "blob"
}

resource "azurerm_cognitive_account" "content_safety" {
  name                  = var.content_safety_name
  location              = var.location
  resource_group_name   = data.azurerm_resource_group.main.name
  kind                  = "ContentSafety"
  sku_name              = "F0"
  custom_subdomain_name = var.content_safety_name
  tags                  = local.tags
}

resource "azurerm_container_app_environment" "main" {
  name                       = "cae-bustrack-ncus"
  location                   = var.location
  resource_group_name        = data.azurerm_resource_group.main.name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id
  tags                       = local.tags
}

resource "azurerm_container_app" "ml" {
  name                         = "bustrack-ml"
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = data.azurerm_resource_group.main.name
  revision_mode                = "Single"
  tags                         = local.tags

  registry {
    server               = local.acr_server
    username             = local.acr_user
    password_secret_name = "acr-pwd"
  }

  secret {
    name  = "acr-pwd"
    value = local.acr_pass
  }

  secret {
    name  = "internal-key"
    value = var.internal_service_key
  }

  secret {
    name  = "ai-cs"
    value = azurerm_application_insights.main.connection_string
  }

  ingress {
    external_enabled = false
    target_port      = 8000
    transport        = "auto"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  template {
    min_replicas = var.internal_min_replicas
    max_replicas = 2

    container {
      name   = "bustrack-ml"
      image  = "${local.acr_server}/bustrack-ml:latest"
      cpu    = 0.5
      memory = "1.0Gi"

      env {
        name  = "ENVIRONMENT"
        value = "production"
      }

      env {
        name        = "INTERNAL_SERVICE_CREDENTIAL"
        secret_name = "internal-key"
      }

      env {
        name        = "APPLICATIONINSIGHTS_CONNECTION_STRING"
        secret_name = "ai-cs"
      }

      liveness_probe {
        transport = "HTTP"
        port      = 8000
        path      = "/health"
      }
    }
  }
}

resource "azurerm_container_app" "genai" {
  name                         = "bustrack-genai"
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = data.azurerm_resource_group.main.name
  revision_mode                = "Single"
  tags                         = local.tags

  registry {
    server               = local.acr_server
    username             = local.acr_user
    password_secret_name = "acr-pwd"
  }

  secret {
    name  = "acr-pwd"
    value = local.acr_pass
  }

  secret {
    name  = "internal-key"
    value = var.internal_service_key
  }

  secret {
    name  = "ai-cs"
    value = azurerm_application_insights.main.connection_string
  }

  ingress {
    external_enabled = false
    target_port      = 8000
    transport        = "auto"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  template {
    min_replicas = var.internal_min_replicas
    max_replicas = 2

    container {
      name   = "bustrack-genai"
      image  = "${local.acr_server}/bustrack-genai:latest"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "ENVIRONMENT"
        value = "production"
      }

      env {
        name        = "INTERNAL_SERVICE_CREDENTIAL"
        secret_name = "internal-key"
      }

      env {
        name        = "APPLICATIONINSIGHTS_CONNECTION_STRING"
        secret_name = "ai-cs"
      }

      liveness_probe {
        transport = "HTTP"
        port      = 8000
        path      = "/health"
      }
    }
  }
}

resource "azurerm_container_app" "landing" {
  name                         = "bustrack-landing"
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = data.azurerm_resource_group.main.name
  revision_mode                = "Single"
  tags                         = local.tags

  registry {
    server               = local.acr_server
    username             = local.acr_user
    password_secret_name = "acr-pwd"
  }

  secret {
    name  = "acr-pwd"
    value = local.acr_pass
  }

  ingress {
    external_enabled = true
    target_port      = 8080
    transport        = "auto"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  template {
    min_replicas = var.landing_min_replicas
    max_replicas = 2

    container {
      name   = "bustrack-landing"
      image  = "${local.acr_server}/bustrack-landing:latest"
      cpu    = 0.25
      memory = "0.5Gi"

      liveness_probe {
        transport = "HTTP"
        port      = 8080
        path      = "/health"
      }
    }
  }
}

resource "azurerm_container_app" "api" {
  name                         = "bustrack-api"
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = data.azurerm_resource_group.main.name
  revision_mode                = "Single"
  tags                         = local.tags

  registry {
    server               = local.acr_server
    username             = local.acr_user
    password_secret_name = "acr-pwd"
  }

  secret {
    name  = "acr-pwd"
    value = local.acr_pass
  }

  secret {
    name  = "mongodb-cs"
    value = var.mongodb_connection_string
  }

  secret {
    name  = "jwt-key"
    value = var.jwt_secret_key
  }

  secret {
    name  = "internal-key"
    value = var.internal_service_key
  }

  secret {
    name  = "cs-key"
    value = azurerm_cognitive_account.content_safety.primary_access_key
  }

  secret {
    name  = "storage-cs"
    value = azurerm_storage_account.media.primary_connection_string
  }

  secret {
    name  = "ai-cs"
    value = azurerm_application_insights.main.connection_string
  }

  ingress {
    external_enabled = true
    target_port      = 8000
    transport        = "auto"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  template {
    min_replicas = var.api_min_replicas
    max_replicas = var.api_max_replicas

    http_scale_rule {
      name                = "http-concurrency"
      concurrent_requests = "25"
    }

    container {
      name   = "bustrack-api"
      image  = "${local.acr_server}/bustrack-api:latest"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "PORT"
        value = "8000"
      }

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }

      env {
        name  = "DataProtection__KeysPath"
        value = "/app/keys"
      }

      env {
        name  = "Persistence__Provider"
        value = "MongoDB"
      }

      env {
        name  = "MongoDb__Database"
        value = "BusTrackNoSQL"
      }

      env {
        name  = "EnableHttpsRedirection"
        value = "false"
      }

      env {
        name        = "MongoDb__ConnectionString"
        secret_name = "mongodb-cs"
      }

      env {
        name        = "Jwt__Key"
        secret_name = "jwt-key"
      }

      env {
        name        = "InternalService__Key"
        secret_name = "internal-key"
      }

      env {
        name  = "ContentSafety__Endpoint"
        value = azurerm_cognitive_account.content_safety.endpoint
      }

      env {
        name        = "ContentSafety__Key"
        secret_name = "cs-key"
      }

      env {
        name        = "AzureStorage__ConnectionString"
        secret_name = "storage-cs"
      }

      env {
        name  = "AzureStorage__MediaContainer"
        value = azurerm_storage_container.media_cdn.name
      }

      env {
        name        = "APPLICATIONINSIGHTS_CONNECTION_STRING"
        secret_name = "ai-cs"
      }

      env {
        name  = "Services__MlBaseUrl"
        value = "https://${azurerm_container_app.ml.ingress[0].fqdn}/"
      }

      env {
        name  = "Services__GenAiBaseUrl"
        value = "https://${azurerm_container_app.genai.ingress[0].fqdn}/"
      }

      env {
        name  = "Cors__AllowedOrigins__0"
        value = "https://${azurerm_container_app.landing.ingress[0].fqdn}"
      }

      liveness_probe {
        transport        = "HTTP"
        port             = 8000
        path             = "/health"
        initial_delay    = 10
        interval_seconds = 30
      }

      readiness_probe {
        transport        = "HTTP"
        port             = 8000
        path             = "/health/ready"
        interval_seconds = 30
      }
    }
  }
}
