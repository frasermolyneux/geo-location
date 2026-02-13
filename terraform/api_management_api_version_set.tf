resource "azurerm_api_management_api_version_set" "api_version_set" {
  name = "geolocation-api"

  resource_group_name = azurerm_api_management.apim.resource_group_name
  api_management_name = azurerm_api_management.apim.name

  display_name      = "GeoLocation Lookup API"
  versioning_scheme = "Segment"
}
