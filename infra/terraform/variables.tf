variable "resource_group_name" {
  type        = string
  description = "Grupo de recursos existente en Azure."
  default     = "rg-bustrack-ncus"
}

variable "location" {
  type        = string
  description = "Región permitida por Azure for Students (northcentralus)."
  default     = "northcentralus"
}

variable "acr_name" {
  type        = string
  description = "Azure Container Registry existente."
  default     = "bustrackacr0102"
}

variable "storage_account_name" {
  type        = string
  description = "Nombre global único para Azure Blob Storage CDN de medios."
  default     = "stbustrackmedia0102"
}

variable "content_safety_name" {
  type        = string
  description = "Recurso Azure AI Content Safety (capa gratuita F0)."
  default     = "cog-bustrack-safety-ncus"
}

variable "api_min_replicas" {
  type        = number
  description = "Mínimo de réplicas de bustrack-api (Requisito 15: >= 2)."
  default     = 2
}

variable "api_max_replicas" {
  type        = number
  description = "Máximo de réplicas para autoescalado HTTP de bustrack-api."
  default     = 4
}

variable "internal_min_replicas" {
  type        = number
  description = "Réplicas mínimas de microservicios internos bustrack-ml y bustrack-genai (0 = scale-to-zero $0)."
  default     = 0
}

variable "landing_min_replicas" {
  type        = number
  description = "Réplicas mínimas de bustrack-landing HTTPS en Container Apps."
  default     = 1
}

variable "enable_front_door" {
  type        = bool
  description = "Opcional Azure Front Door (~$35/mes); desactivado por defecto para preservar crédito."
  default     = false
}

variable "mongodb_connection_string" {
  type        = string
  sensitive   = true
  description = "Cadena SRV hacia MongoDB Atlas (BusTrackNoSQL)."
}

variable "jwt_secret_key" {
  type        = string
  sensitive   = true
  description = "Llave simétrica HS256 >= 32 bytes."
}

variable "internal_service_key" {
  type        = string
  sensitive   = true
  description = "Credencial compartida X-Internal-Key entre API, ML y GenAI."
}

variable "redis_connection_string" {
  type        = string
  sensitive   = true
  default     = ""
  description = "Cadena TLS opcional para Redis distribuido (Upstash); vacío usa caché distribuido en memoria."
}

variable "firebase_credentials_json" {
  type        = string
  sensitive   = true
  default     = ""
  description = "Service Account JSON opcional de Firebase Cloud Messaging."
}
