workload    = "geo-location"
environment = "prd"
location    = "swedencentral"

subscription_id = "903b6685-c12a-4703-ac54-7ec1ff15ca43"

log_analytics_subscription_id     = "d68448b0-9947-46d7-8771-baa331a3063a"
log_analytics_resource_group_name = "rg-platform-logging-prd-uksouth-01"
log_analytics_workspace_name      = "log-platform-prd-uksouth-01"

platform_hosting_state = {
  resource_group_name  = "rg-tf-platform-hosting-prd-uksouth-01"
  storage_account_name = "sab227d365059d"
  container_name       = "tfstate"
  key                  = "terraform.tfstate"
  subscription_id      = "7760848c-794d-4a19-8cb2-52f71a21ac2b"
  tenant_id            = "e56a6947-bb9a-4a6e-846a-1f118d1c3a14"
  use_oidc             = true
}

dns = {
  subscription_id     = "db34f572-8b71-40d6-8f99-f29a27612144"
  resource_group_name = "rg-platform-dns-prd-uksouth-01"
  domain              = "geo-location.net"
  web_subdomain       = "www"
}

api_consumers = [
  {
    workload     = "portal-repository-func-dev"
    principal_id = "bec26e1f-d66c-4234-aa3a-ca96ae0f61de"
  },
  {
    workload     = "portal-repository-func-prd"
    principal_id = "737442a4-9953-45fb-8fdb-425b63c7963d"
  },
  {
    workload     = "portal-web-dev"
    principal_id = "640262c1-e930-4ec1-8ffc-d47caa32ead7"
  },
  {
    workload     = "portal-web-prd"
    principal_id = "37bbe97d-def7-4cdd-9ea4-31bce4f23d5f"
  }
]

tags = {
  Environment = "prd"
  Workload    = "geo-location"
  DeployedBy  = "GitHub-Terraform"
  Git         = "https://github.com/frasermolyneux/geo-location"
}
