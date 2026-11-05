locals {
  production_alerts_enabled = var.environment == "prd"
}

resource "azurerm_monitor_metric_alert" "app_service_plan_cpu" {
  count = local.production_alerts_enabled ? 1 : 0

  name                = "${var.workload}-${var.environment}-app-service-plan-cpu"
  resource_group_name = data.azurerm_resource_group.rg.name
  scopes              = [local.app_service_plan_id]
  description         = "Shared App Service plan CPU averaged at least 80% over five minutes."
  severity            = 1
  frequency           = "PT1M"
  window_size         = "PT5M"

  criteria {
    metric_namespace = "Microsoft.Web/serverfarms"
    metric_name      = "CpuPercentage"
    aggregation      = "Average"
    operator         = "GreaterThanOrEqual"
    threshold        = 80
  }

  action {
    action_group_id = var.alert_action_group_id
  }

  tags = var.tags
}

resource "azurerm_monitor_metric_alert" "app_service_plan_memory" {
  count = local.production_alerts_enabled ? 1 : 0

  name                = "${var.workload}-${var.environment}-app-service-plan-memory"
  resource_group_name = data.azurerm_resource_group.rg.name
  scopes              = [local.app_service_plan_id]
  description         = "Shared App Service plan memory averaged at least 90% over five minutes."
  severity            = 1
  frequency           = "PT1M"
  window_size         = "PT5M"

  criteria {
    metric_namespace = "Microsoft.Web/serverfarms"
    metric_name      = "MemoryPercentage"
    aggregation      = "Average"
    operator         = "GreaterThanOrEqual"
    threshold        = 90
  }

  action {
    action_group_id = var.alert_action_group_id
  }

  tags = var.tags
}

resource "azurerm_monitor_metric_alert" "web_app_latency" {
  for_each = local.production_alerts_enabled ? local.monitored_web_apps : {}

  name                = "${var.workload}-${var.environment}-${each.key}-latency"
  resource_group_name = data.azurerm_resource_group.rg.name
  scopes              = [each.value]
  description         = "${each.key} response time averaged more than five seconds over five minutes."
  severity            = 1
  frequency           = "PT1M"
  window_size         = "PT5M"

  criteria {
    metric_namespace = "Microsoft.Web/sites"
    metric_name      = "HttpResponseTime"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = 5
  }

  action {
    action_group_id = var.alert_action_group_id
  }

  tags = var.tags
}

resource "azurerm_monitor_metric_alert" "web_app_failures" {
  for_each = local.production_alerts_enabled ? local.monitored_web_apps : {}

  name                = "${var.workload}-${var.environment}-${each.key}-http-5xx"
  resource_group_name = data.azurerm_resource_group.rg.name
  scopes              = [each.value]
  description         = "${each.key} returned at least five server errors in five minutes."
  severity            = 1
  frequency           = "PT1M"
  window_size         = "PT5M"

  criteria {
    metric_namespace = "Microsoft.Web/sites"
    metric_name      = "Http5xx"
    aggregation      = "Total"
    operator         = "GreaterThanOrEqual"
    threshold        = 5
  }

  action {
    action_group_id = var.alert_action_group_id
  }

  tags = var.tags
}
