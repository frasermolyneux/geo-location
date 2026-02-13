resource "azurerm_api_management_subscription" "consumers" {
  for_each = { for c in var.api_consumers : c.workload => c }

  api_management_name = azurerm_api_management.apim.name
  resource_group_name = data.azurerm_resource_group.rg.name
  display_name        = each.value.workload
  product_id          = azurerm_api_management_product.api_product.id
  state               = "active"
}

resource "random_id" "consumer" {
  for_each    = { for c in var.api_consumers : c.workload => c }
  byte_length = 6

  keepers = {
    workload = each.value.workload
  }
}

resource "azurerm_key_vault" "consumer" {
  for_each = { for c in var.api_consumers : c.workload => c }

  name                = "kv-${random_id.consumer[each.key].hex}-${local.location_short}"
  location            = data.azurerm_resource_group.rg.location
  resource_group_name = data.azurerm_resource_group.rg.name
  tenant_id           = data.azuread_client_config.current.tenant_id
  sku_name            = "standard"

  rbac_authorization_enabled = true

  tags = merge(var.tags, {
    consumerWorkload    = each.value.workload
    consumerPrincipalId = each.value.principal_id
  })
}

resource "azurerm_role_assignment" "consumer_kv_secrets_user" {
  for_each = { for c in var.api_consumers : c.workload => c }

  scope                = azurerm_key_vault.consumer[each.key].id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = each.value.principal_id
}

resource "azurerm_role_assignment" "consumer_kv_deploy_secrets_officer" {
  for_each = { for c in var.api_consumers : c.workload => c }

  scope                = azurerm_key_vault.consumer[each.key].id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = data.azuread_client_config.current.object_id
}

resource "azurerm_key_vault_secret" "consumer_apim_key" {
  for_each = { for c in var.api_consumers : c.workload => c }

  name         = "${each.value.workload}-apim-subscription-key"
  value        = azurerm_api_management_subscription.consumers[each.key].primary_key
  key_vault_id = azurerm_key_vault.consumer[each.key].id

  depends_on = [azurerm_role_assignment.consumer_kv_deploy_secrets_officer]
}

# App role assignments for API consumers to access API
resource "azuread_app_role_assignment" "consumer_to_api" {
  for_each = { for c in var.api_consumers : c.workload => c }

  app_role_id         = local.lookup_api_user_role_id
  principal_object_id = each.value.principal_id
  resource_object_id  = azuread_service_principal.api.object_id
}
