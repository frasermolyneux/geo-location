resource "azurerm_linux_web_app" "api" {
  name = local.api_app_name
  tags = var.tags

  resource_group_name = local.platform_hosting_app_service_plan.resource_group_name
  location            = local.platform_hosting_app_service_plan.location

  service_plan_id = local.platform_hosting_app_service_plan.id

  https_only = true

  identity {
    type = "SystemAssigned"
  }

  site_config {
    application_stack {
      dotnet_version = "9.0"
    }

    always_on                         = true
    ftps_state                        = "Disabled"
    minimum_tls_version               = "1.2"
    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
  }

  app_settings = {
    "APPLICATIONINSIGHTS_CONNECTION_STRING"      = azurerm_application_insights.ai.connection_string
    "ApplicationInsightsAgent_EXTENSION_VERSION" = "~3"
    "ASPNETCORE_ENVIRONMENT"                     = var.environment == "prd" ? "Production" : "Development"
    "WEBSITE_RUN_FROM_PACKAGE"                   = "1"
    "AzureAd__TenantId"                          = data.azuread_client_config.current.tenant_id
    "AzureAd__Instance"                          = "https://login.microsoftonline.com/"
    "AzureAd__ClientId"                          = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.kv.name};SecretName=geolocation-api-${var.environment}-clientid)"
    "AzureAd__ClientSecret"                      = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.kv.name};SecretName=geolocation-api-${var.environment}-clientsecret)"
    "AzureAd__Audience"                          = local.entra_api_identifier_uri
    "maxmind_apikey"                             = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.kv.name};SecretName=maxmind-apikey)"
    "maxmind_userid"                             = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.kv.name};SecretName=maxmind-userid)"
    "Storage__TableEndpoint"                     = azurerm_storage_account.data.primary_table_endpoint
    "APPINSIGHTS_PROFILERFEATURE_VERSION"        = "1.0.0"
    "DiagnosticServices_EXTENSION_VERSION"       = "~3"
  }
}
