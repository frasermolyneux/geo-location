// Backend for the API
resource "azurerm_api_management_backend" "api_backend" {
  name = local.api_app_name

  resource_group_name = azurerm_api_management.apim.resource_group_name
  api_management_name = azurerm_api_management.apim.name

  protocol    = "http"
  title       = local.api_app_name
  description = "Backend for GeoLocation Lookup API"
  url         = format("https://%s", azurerm_linux_web_app.api.default_hostname)

  tls {
    validate_certificate_chain = true
    validate_certificate_name  = true
  }
}

resource "azurerm_api_management_logger" "app_insights" {
  name                = "${var.workload}-application-insights"
  resource_group_name = azurerm_api_management.apim.resource_group_name
  api_management_name = azurerm_api_management.apim.name

  application_insights {
    instrumentation_key = azurerm_application_insights.ai.instrumentation_key
  }
}
