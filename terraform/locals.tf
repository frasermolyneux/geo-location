locals {
  resource_group_name               = "rg-${var.workload}-${var.environment}-${var.location}"
  platform_hosting_app_service_plan = data.terraform_remote_state.platform_hosting.outputs.app_service_plans["default"]
  platform_monitoring_workspace_id  = data.terraform_remote_state.platform_monitoring.outputs.log_analytics.id

  # Location abbreviations for resource names with strict length limits
  location_short = substr(var.location, 0, 3)

  app_insights_name = "ai-${var.workload}-${var.environment}-${var.location}"

  api_app_name = "app-${var.workload}-api-${var.environment}-${var.location}-${random_id.environment_id.hex}"
  web_app_name = "app-${var.workload}-web-${var.environment}-${var.location}-${random_id.environment_id.hex}"

  key_vault_name = "kv-${random_id.environment_id.hex}-${local.location_short}"

  storage_account_prefix = "sageoloc"
  storage_account_name   = lower("${local.storage_account_prefix}${var.environment}${random_id.storage.hex}")

  public_hostname = "${var.dns.web_subdomain}.${var.dns.domain}"

  api_management_name      = "apim-${var.workload}-${var.environment}-${var.location}-${random_id.environment_id.hex}"
  api_management_root_path = "geolocation-api"

  app_insights_sampling_percentage = {
    dev = 25
    prd = 75
  }

  entra_api_app_display_name = "geolocation-api-${var.environment}"
  entra_api_identifier_uri   = format("api://%s/%s", data.azuread_client_config.current.tenant_id, local.entra_api_app_display_name)
  entra_web_app_display_name = "geolocation-web-${var.environment}"

  # API app role ID for LookupApiUser
  lookup_api_user_role_id = "b4b62713-44f8-4871-8c10-2c85369b776d"

  entra_web_redirect_uris = distinct([
    "https://${local.public_hostname}/signin-oidc",
    "https://${local.web_app_name}.azurewebsites.net/signin-oidc",
    "https://localhost:5001/signin-oidc"
  ])
  entra_web_logout_url = "https://${local.public_hostname}/signout-callback-oidc"
}
