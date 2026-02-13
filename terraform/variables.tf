variable "workload" {
  default = "geo-location"
}

variable "environment" {
  default = "dev"
}

variable "location" {
  default = "swedencentral"
}

variable "subscription_id" {}

variable "platform_monitoring_state" {
  description = "Backend config for platform-monitoring remote state"
  type = object({
    resource_group_name  = string
    storage_account_name = string
    container_name       = string
    key                  = string
    subscription_id      = string
    tenant_id            = string
  })
}

variable "platform_hosting_state" {
  description = "Backend config for platform-hosting remote state (shared app service plan)."
  type = object({
    resource_group_name  = string
    storage_account_name = string
    container_name       = string
    key                  = string
    subscription_id      = string
    tenant_id            = string
    use_oidc             = bool
  })
}

variable "dns" {
  type = object({
    subscription_id     = string
    resource_group_name = string
    domain              = string
    web_subdomain       = string
  })
}

variable "api_consumers" {
  description = "External API consumers that need APIM subscriptions."
  type = list(object({
    workload     = string
    principal_id = string
  }))
  default = []
}

variable "tags" {
  default = {}
}
