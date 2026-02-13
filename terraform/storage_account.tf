resource "azurerm_storage_account" "data" {
  name                            = local.storage_account_name
  resource_group_name             = data.azurerm_resource_group.rg.name
  location                        = data.azurerm_resource_group.rg.location
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  min_tls_version                 = "TLS1_2"
  allow_nested_items_to_be_public = false
  tags                            = var.tags
}

resource "azurerm_storage_table" "geolocations" {
  name                 = "geolocations"
  storage_account_name = azurerm_storage_account.data.name
}

resource "azurerm_role_assignment" "api_table_data_contributor" {
  scope                = azurerm_storage_account.data.id
  role_definition_name = "Storage Table Data Contributor"
  principal_id         = azurerm_linux_web_app.api.identity[0].principal_id
}
