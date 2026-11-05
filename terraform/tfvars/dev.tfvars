workload    = "geo-location"
environment = "dev"
location    = "swedencentral"

subscription_id = "6cad03c1-9e98-4160-8ebe-64dd30f1bbc7"

noncritical_monitoring_workspace_id = "/subscriptions/6cad03c1-9e98-4160-8ebe-64dd30f1bbc7/resourceGroups/rg-platform-monitoring-noncritical-prd-uksouth/providers/Microsoft.OperationalInsights/workspaces/log-platform-monitoring-noncritical-prd-uksouth"

dns = {
  subscription_id     = "db34f572-8b71-40d6-8f99-f29a27612144"
  resource_group_name = "rg-platform-dns-prd-uksouth-01"
  domain              = "geo-location.net"
  web_subdomain       = "dev"
}

api_consumers = []

tags = {
  Environment = "dev"
  Workload    = "geo-location"
  DeployedBy  = "GitHub-Terraform"
  Git         = "https://github.com/frasermolyneux/geo-location"
}

gcp_project_id = "gcp-mx-io-geo-location"

google_maps_allowed_referrers = [
  "https://dev.geo-location.net/*"
]
