locals {
  // List of version files that exist (excluding legacy which is handled separately)
  version_files = fileset("../openapi", "openapi-v*.json")

  // Extract version strings from filenames (e.g., "v1")
  // Filter out the old v1 file
  version_strings = [for file in local.version_files :
    trimsuffix(trimprefix(basename(file), "openapi-"), ".json")
    if !strcontains(file, "-old")
  ]

  // Only versioned APIs (no legacy)
  versioned_apis = [for version in local.version_strings :
    version if version != "legacy"
  ]
}

// Data sources for versioned OpenAPI specification files
data "local_file" "openapi_versioned" {
  for_each = toset(local.versioned_apis)
  filename = "../openapi/openapi-${each.key}.json"
}

// Data source for legacy OpenAPI specification
data "local_file" "openapi_legacy" {
  filename = "../openapi/openapi-legacy.json"
}

// Backend for the API
resource "azurerm_api_management_backend" "api_backend" {
  name = local.api_app_name

  resource_group_name = azurerm_api_management.apim.resource_group_name
  api_management_name = azurerm_api_management.apim.name

  protocol    = "http"
  title       = local.api_app_name
  description = "Backend for GeoLocation Lookup API"
  url         = format("https://%s/api", azurerm_linux_web_app.api.default_hostname)

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

// Dynamic versioned APIs that are discovered from OpenAPI spec files
resource "azurerm_api_management_api" "versioned_api" {
  for_each = toset(local.versioned_apis)

  name = "geolocation-api-${replace(each.key, ".", "-")}"

  resource_group_name = azurerm_api_management.apim.resource_group_name
  api_management_name = azurerm_api_management.apim.name

  revision     = "1"
  display_name = "GeoLocation Lookup API"
  description  = "API for geolocation lookup"
  path         = "geolocation"
  protocols    = ["https"]

  subscription_required = true

  subscription_key_parameter_names {
    header = "Ocp-Apim-Subscription-Key"
    query  = "subscription-key"
  }

  version        = each.key
  version_set_id = azurerm_api_management_api_version_set.api_version_set.id

  import {
    content_format = "openapi+json"
    content_value  = data.local_file.openapi_versioned[each.key].content
  }
}

// Add versioned APIs to the product
resource "azurerm_api_management_product_api" "versioned_api" {
  for_each = azurerm_api_management_api.versioned_api

  api_name   = each.value.name
  product_id = azurerm_api_management_product.api_product.product_id

  resource_group_name = azurerm_api_management.apim.resource_group_name
  api_management_name = azurerm_api_management.apim.name
}

// Configure policies for versioned APIs
resource "azurerm_api_management_api_policy" "versioned_api_policy" {
  for_each = azurerm_api_management_api.versioned_api

  api_name = each.value.name

  resource_group_name = azurerm_api_management.apim.resource_group_name
  api_management_name = azurerm_api_management.apim.name

  xml_content = <<XML
<policies>
  <inbound>
      <base/>
      <set-backend-service backend-id="${azurerm_api_management_backend.api_backend.name}" />
      <set-variable name="rewriteUriTemplate" value="@((string)context.Request.OriginalUrl.Path.Substring(context.Api.Path.Length))" />
      <rewrite-uri template="@((string)context.Variables["rewriteUriTemplate"])" />
  </inbound>
  <backend>
      <forward-request />
  </backend>
  <outbound>
      <base/>
  </outbound>
  <on-error />
</policies>
XML

  depends_on = [
    azurerm_api_management_backend.api_backend
  ]
}

resource "azurerm_api_management_api_diagnostic" "versioned_api_diagnostic" {
  for_each = azurerm_api_management_api.versioned_api

  api_name                 = each.value.name
  identifier               = "applicationinsights"
  resource_group_name      = azurerm_api_management.apim.resource_group_name
  api_management_name      = azurerm_api_management.apim.name
  api_management_logger_id = azurerm_api_management_logger.app_insights.id
}

// Legacy API (no version in path, rewrites to /v1)
resource "azurerm_api_management_api" "legacy_api" {
  name = "geolocation-api-legacy"

  resource_group_name = azurerm_api_management.apim.resource_group_name
  api_management_name = azurerm_api_management.apim.name

  revision     = "1"
  display_name = "GeoLocation Lookup API"
  description  = "API for geolocation lookup (legacy)"
  path         = "geolocation"
  protocols    = ["https"]

  subscription_required = true

  subscription_key_parameter_names {
    header = "Ocp-Apim-Subscription-Key"
    query  = "subscription-key"
  }

  version        = ""
  version_set_id = azurerm_api_management_api_version_set.api_version_set.id

  import {
    content_format = "openapi+json"
    content_value  = data.local_file.openapi_legacy.content
  }
}

resource "azurerm_api_management_product_api" "legacy_api" {
  api_name   = azurerm_api_management_api.legacy_api.name
  product_id = azurerm_api_management_product.api_product.product_id

  resource_group_name = azurerm_api_management.apim.resource_group_name
  api_management_name = azurerm_api_management.apim.name
}

resource "azurerm_api_management_api_policy" "legacy_api_policy" {
  api_name = azurerm_api_management_api.legacy_api.name

  resource_group_name = azurerm_api_management.apim.resource_group_name
  api_management_name = azurerm_api_management.apim.name

  xml_content = <<XML
<policies>
  <inbound>
      <base/>
      <set-backend-service backend-id="${azurerm_api_management_backend.api_backend.name}" />
      <set-variable name="rewriteUriTemplate" value="@(&quot;/v1&quot; + context.Request.OriginalUrl.Path.Substring(context.Api.Path.Length))" />
      <rewrite-uri template="@((string)context.Variables[&quot;rewriteUriTemplate&quot;])" />
  </inbound>
  <backend>
      <forward-request />
  </backend>
  <outbound>
      <base/>
  </outbound>
  <on-error />
</policies>
XML

  depends_on = [azurerm_api_management_backend.api_backend]
}

resource "azurerm_api_management_api_diagnostic" "legacy_api_diagnostic" {
  api_name                 = azurerm_api_management_api.legacy_api.name
  identifier               = "applicationinsights"
  resource_group_name      = azurerm_api_management.apim.resource_group_name
  api_management_name      = azurerm_api_management.apim.name
  api_management_logger_id = azurerm_api_management_logger.app_insights.id
}
