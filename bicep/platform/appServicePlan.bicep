targetScope = 'resourceGroup'

param parAppServicePlanName string
param parLocation string

resource appServicePlan 'Microsoft.Web/serverfarms@2020-10-01' = {
  name: parAppServicePlanName
  location: parLocation

  sku: {
    name: 'D1'
    tier: 'Shared'
  }
}

output outAppServicePlanId string = appServicePlan.id
output outAppServicePlanName string = appServicePlan.name
