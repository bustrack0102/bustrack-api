output "api_https_url" {
  value       = "https://${azurerm_container_app.api.ingress[0].fqdn}"
  description = "URL HTTPS pública detrás de Envoy Load Balancer (min_replicas >= 2)."
}

output "landing_https_url" {
  value       = "https://${azurerm_container_app.landing.ingress[0].fqdn}"
  description = "URL HTTPS pública del portal Landing y avisos legales."
}

output "ml_internal_fqdn" {
  value       = azurerm_container_app.ml.ingress[0].fqdn
  description = "FQDN interno (no expuesto a internet) de bustrack-ml."
}

output "genai_internal_fqdn" {
  value       = azurerm_container_app.genai.ingress[0].fqdn
  description = "FQDN interno (no expuesto a internet) de bustrack-genai."
}

output "app_insights_name" {
  value       = azurerm_application_insights.main.name
  description = "Instancia Application Insights conectada a Log Analytics."
}

output "media_storage_account" {
  value       = azurerm_storage_account.media.name
  description = "Cuenta Azure Blob Storage CDN para imágenes y videos moderados."
}

output "content_safety_endpoint" {
  value       = azurerm_cognitive_account.content_safety.endpoint
  description = "Endpoint Azure AI Content Safety F0."
}
