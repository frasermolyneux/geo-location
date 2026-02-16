output "resource_group_name" {
  value = data.azurerm_resource_group.rg.name
}

output "api_app_name" {
  value = azurerm_linux_web_app.api.name
}

output "api_app_resource_group_name" {
  value = azurerm_linux_web_app.api.resource_group_name
}

output "web_app_name" {
  value = azurerm_linux_web_app.web.name
}

output "web_app_resource_group_name" {
  value = azurerm_linux_web_app.web.resource_group_name
}

output "key_vault_name" {
  value = azurerm_key_vault.kv.name
}

output "api_management_name" {
  value = azurerm_api_management.apim.name
}

output "api_management_resource_group_name" {
  value = azurerm_api_management.apim.resource_group_name
}

output "api_version_set_id" {
  value = azurerm_api_management_api_version_set.api_version_set.id
}

output "api_management_product_id" {
  value = azurerm_api_management_product.api_product.product_id
}

output "entra_api_application_client_id" {
  value = azuread_application.api.client_id
}

output "entra_web_application_client_id" {
  value = azuread_application.web.client_id
}

output "storage_account_name" {
  value = azurerm_storage_account.data.name
}
