resource "azurerm_api_management_product" "api_product" {
  product_id = "geolocation-api"

  resource_group_name = azurerm_api_management.apim.resource_group_name
  api_management_name = azurerm_api_management.apim.name

  display_name = "GeoLocation Lookup API"

  subscription_required = true
  approval_required     = false
  published             = true
}
