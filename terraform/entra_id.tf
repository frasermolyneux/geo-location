# API App Registration (Lookup API)
resource "azuread_application" "api" {
  display_name     = local.entra_api_app_display_name
  description      = "GeoLocation Lookup API"
  sign_in_audience = "AzureADMyOrg"

  identifier_uris = [local.entra_api_identifier_uri]

  app_role {
    allowed_member_types = ["Application"]
    description          = "Allows applications to perform lookup requests"
    display_name         = "LookupApiUser"
    id                   = "b4b62713-44f8-4871-8c10-2c85369b776d"
    value                = "LookupApiUser"
    enabled              = true
  }

  web {
    implicit_grant {
      access_token_issuance_enabled = false
      id_token_issuance_enabled     = true
    }
  }

  prevent_duplicate_names = true
}

resource "azuread_service_principal" "api" {
  client_id                    = azuread_application.api.client_id
  app_role_assignment_required = false

  owners = [
    data.azuread_client_config.current.object_id
  ]
}

resource "azuread_application_password" "api" {
  application_id = azuread_application.api.id

  rotate_when_changed = {
    rotation = time_rotating.thirty_days.id
  }
}

resource "azurerm_key_vault_secret" "api_client_id" {
  name         = "geolocation-api-${var.environment}-clientid"
  value        = azuread_application.api.client_id
  key_vault_id = azurerm_key_vault.kv.id

  depends_on = [azurerm_role_assignment.deploy_kv_secrets_officer]
}

resource "azurerm_key_vault_secret" "api_client_secret" {
  name         = "geolocation-api-${var.environment}-clientsecret"
  value        = azuread_application_password.api.value
  key_vault_id = azurerm_key_vault.kv.id

  depends_on = [azurerm_role_assignment.deploy_kv_secrets_officer]
}

# Web App Registration
resource "azuread_application" "web" {
  display_name     = local.entra_web_app_display_name
  description      = "GeoLocation Web front-end"
  sign_in_audience = "AzureADMyOrg"

  web {
    homepage_url  = "https://${local.public_hostname}/"
    logout_url    = local.entra_web_logout_url
    redirect_uris = local.entra_web_redirect_uris

    implicit_grant {
      access_token_issuance_enabled = false
      id_token_issuance_enabled     = true
    }
  }

  required_resource_access {
    resource_app_id = azuread_application.api.client_id

    resource_access {
      id   = "b4b62713-44f8-4871-8c10-2c85369b776d"
      type = "Role"
    }
  }

  prevent_duplicate_names = true
}

resource "azuread_service_principal" "web" {
  client_id                    = azuread_application.web.client_id
  app_role_assignment_required = false

  owners = [
    data.azuread_client_config.current.object_id
  ]
}

resource "azuread_application_password" "web" {
  application_id = azuread_application.web.id

  rotate_when_changed = {
    rotation = time_rotating.thirty_days.id
  }
}

# App role assignments for Web App managed identity to access API
resource "azuread_app_role_assignment" "web_to_api" {
  app_role_id         = "b4b62713-44f8-4871-8c10-2c85369b776d" # LookupApiUser role
  principal_object_id = azurerm_linux_web_app.web.identity[0].principal_id
  resource_object_id  = azuread_service_principal.api.object_id
}
