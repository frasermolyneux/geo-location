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

resource "azurerm_key_vault_secret" "storage_connection_string" {
  name         = "${azurerm_storage_account.data.name}-connectionstring"
  value        = azurerm_storage_account.data.primary_connection_string
  key_vault_id = azurerm_key_vault.kv.id

  depends_on = [azurerm_role_assignment.deploy_kv_secrets_officer]
}
