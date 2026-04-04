workload    = "geo-location"
environment = "prd"
location    = "swedencentral"

subscription_id = "903b6685-c12a-4703-ac54-7ec1ff15ca43"

platform_monitoring_state = {
  resource_group_name  = "rg-tf-platform-monitoring-prd-uksouth-01"
  storage_account_name = "sa74f04c5f984e"
  container_name       = "tfstate"
  key                  = "terraform.tfstate"
  subscription_id      = "7760848c-794d-4a19-8cb2-52f71a21ac2b"
  tenant_id            = "e56a6947-bb9a-4a6e-846a-1f118d1c3a14"
}

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
    workload      = "portal-repository-func-dev"
    identity_name = "id-portal-repository-func-dev"
  },
  {
    workload      = "portal-repository-func-prd"
    identity_name = "id-portal-repository-func-prd"
  },
  {
    workload      = "portal-web-dev"
    identity_name = "id-portal-web-dev"
  },
  {
    workload      = "portal-web-prd"
    identity_name = "id-portal-web-prd"
  }
]

tags = {
  Environment = "prd"
  Workload    = "geo-location"
  DeployedBy  = "GitHub-Terraform"
  Git         = "https://github.com/frasermolyneux/geo-location"
}

gcp_project_id = "gcp-mx-io-geo-location"
