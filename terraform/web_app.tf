resource "azurerm_linux_web_app" "web" {
  name = local.web_app_name
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

    always_on           = true
    ftps_state          = "Disabled"
    minimum_tls_version = "1.2"
  }

  app_settings = {
    "APPLICATIONINSIGHTS_CONNECTION_STRING"      = azurerm_application_insights.ai.connection_string
    "ApplicationInsightsAgent_EXTENSION_VERSION" = "~3"
    "ASPNETCORE_ENVIRONMENT"                     = var.environment == "prd" ? "Production" : "Development"
    "WEBSITE_RUN_FROM_PACKAGE"                   = "1"
    "GeoLocationApi__BaseUrl"                    = "${azurerm_api_management.apim.gateway_url}/geolocation"
    "GeoLocationApi__ApiKey"                     = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.web_apim_subscription_key.versionless_id})"
    "GeoLocationApi__ApplicationAudience"        = local.entra_api_identifier_uri
    "APPINSIGHTS_PROFILERFEATURE_VERSION"        = "1.0.0"
    "DiagnosticServices_EXTENSION_VERSION"       = "~3"
  }
}

resource "azurerm_api_management_subscription" "web" {
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = data.azurerm_resource_group.rg.name
  display_name        = local.web_app_name
  product_id          = azurerm_api_management_product.api_product.id
  state               = "active"
}

resource "azurerm_key_vault_secret" "web_apim_subscription_key" {
  name         = "${local.web_app_name}-apim-subscription-key"
  value        = azurerm_api_management_subscription.web.primary_key
  key_vault_id = azurerm_key_vault.kv.id

  depends_on = [azurerm_role_assignment.deploy_kv_secrets_officer]
}

resource "azurerm_app_service_custom_hostname_binding" "web" {
  hostname            = local.public_hostname
  app_service_name    = azurerm_linux_web_app.web.name
  resource_group_name = azurerm_linux_web_app.web.resource_group_name

  depends_on = [
    azurerm_dns_txt_record.web_verification,
    azurerm_dns_cname_record.web
  ]
}

resource "time_sleep" "wait_for_hostname_binding" {
  create_duration = "60s"

  depends_on = [
    azurerm_app_service_custom_hostname_binding.web
  ]
}

resource "azurerm_app_service_managed_certificate" "web" {
  custom_hostname_binding_id = azurerm_app_service_custom_hostname_binding.web.id

  depends_on = [
    time_sleep.wait_for_hostname_binding,
    azurerm_dns_cname_record.web
  ]
}

resource "azurerm_app_service_certificate_binding" "web" {
  hostname_binding_id = azurerm_app_service_custom_hostname_binding.web.id
  certificate_id      = azurerm_app_service_managed_certificate.web.id
  ssl_state           = "SniEnabled"
}
