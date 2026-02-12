locals {
  resource_group_name               = "rg-${var.workload}-${var.environment}-${var.location}"
  platform_hosting_app_service_plan = data.terraform_remote_state.platform_hosting.outputs.app_service_plans["default"]

  app_insights_name = "ai-${var.workload}-${var.environment}-${var.location}"

  api_app_name = "app-${var.workload}-api-${var.environment}-${var.location}-${random_id.environment_id.hex}"
  web_app_name = "app-${var.workload}-web-${var.environment}-${var.location}-${random_id.environment_id.hex}"

  key_vault_name = "kv-${random_id.environment_id.hex}"

  storage_account_prefix = "sageoloc"
  storage_account_name   = lower("${local.storage_account_prefix}${var.environment}${random_id.storage.hex}")

  public_hostname = "${var.dns.web_subdomain}.${var.dns.domain}"

  api_management_name = "apim-${var.workload}-${var.environment}-${var.location}-${random_id.environment_id.hex}"

  entra_api_app_display_name = "geolocation-api-${var.environment}"
  entra_api_identifier_uri   = format("api://%s/%s", data.azuread_client_config.current.tenant_id, local.entra_api_app_display_name)
  entra_web_app_display_name = "geolocation-web-${var.environment}"

  entra_web_redirect_uris = distinct([
    "https://${local.public_hostname}/signin-oidc",
    "https://${local.web_app_name}.azurewebsites.net/signin-oidc",
    "https://localhost:5001/signin-oidc"
  ])
  entra_web_logout_url = "https://${local.public_hostname}/signout-callback-oidc"
}
