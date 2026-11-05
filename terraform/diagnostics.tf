locals {
  monitored_web_apps = {
    api = azurerm_linux_web_app.api.id
    web = azurerm_linux_web_app.web.id
  }
}

resource "azurerm_monitor_diagnostic_setting" "web_app" {
  for_each = local.monitored_web_apps

  name                       = "send-to-platform-monitoring"
  target_resource_id         = each.value
  log_analytics_workspace_id = local.platform_monitoring_workspace_id

  enabled_log {
    category = "AppServiceHTTPLogs"
  }

  enabled_log {
    category = "AppServiceConsoleLogs"
  }

  enabled_log {
    category = "AppServiceAppLogs"
  }

  enabled_log {
    category = "AppServicePlatformLogs"
  }

  enabled_metric {
    category = "AllMetrics"
  }
}
