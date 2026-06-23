resource "google_apikeys_key" "maps" {
  # Include environment random suffix to avoid collisions with legacy manually-created keys.
  name         = format("maps-%s-%s", var.environment, random_id.environment_id.hex)
  display_name = format("Geo Location Maps API Key - %s", var.environment)
  project      = var.gcp_project_id

  restrictions {
    api_targets {
      service = "maps-backend.googleapis.com"
    }

    browser_key_restrictions {
      allowed_referrers = var.google_maps_allowed_referrers
    }
  }
}

resource "azurerm_key_vault_secret" "google_maps_api_key" {
  name         = "google-maps-api-key"
  value        = google_apikeys_key.maps.key_string
  key_vault_id = azurerm_key_vault.kv.id

  content_type = "text/plain"

  depends_on = [azurerm_role_assignment.deploy_kv_secrets_officer]
}
