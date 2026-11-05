variable "workload" {
  default = "geo-location"
}

variable "environment" {
  default = "dev"

  validation {
    condition     = contains(["dev", "prd"], var.environment)
    error_message = "environment must be either dev or prd."
  }
}

variable "location" {
  default = "swedencentral"
}

variable "subscription_id" {}

variable "alert_action_group_id" {
  description = "Action group used for production performance and reliability alerts."
  type        = string
  default     = null
  nullable    = true

  validation {
    condition     = var.environment != "prd" || var.alert_action_group_id != null
    error_message = "alert_action_group_id must be set for production."
  }
}

variable "noncritical_monitoring_workspace_id" {
  description = "Resource ID of the platform noncritical Log Analytics workspace used by GeoLocation telemetry."
  type        = string

  validation {
    condition     = can(regex("(?i)^/subscriptions/[0-9a-f-]{36}/resourceGroups/rg-platform-monitoring-noncritical-[^/]+/providers/Microsoft\\.OperationalInsights/workspaces/log-platform-monitoring-noncritical-[^/]+$", var.noncritical_monitoring_workspace_id))
    error_message = "noncritical_monitoring_workspace_id must reference a platform noncritical Log Analytics workspace."
  }
}

variable "platform_hosting_state" {
  description = "Backend config for the production platform-hosting remote state (shared app service plan)."
  type = object({
    resource_group_name  = string
    storage_account_name = string
    container_name       = string
    key                  = string
    subscription_id      = string
    tenant_id            = string
    use_oidc             = bool
  })
  default  = null
  nullable = true

  validation {
    condition     = var.environment != "prd" || var.platform_hosting_state != null
    error_message = "platform_hosting_state must be set for production."
  }
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
  description = "External API consumers that need APIM subscriptions. identity_name is the display name of the consumer's managed identity."
  type = list(object({
    workload      = string
    identity_name = string
  }))
  default = []
}

variable "tags" {
  default = {}
}

variable "gcp_project_id" {
  description = "GCP project ID for Google Maps API key management"
  type        = string
}

variable "google_maps_allowed_referrers" {
  description = "Allowed HTTP referrer domains for the Google Maps API key"
  type        = list(string)
}
