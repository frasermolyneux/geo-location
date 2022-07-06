targetScope = 'resourceGroup'

param parAppServicePlanName string
param parLocation string
param parTags object

resource appServicePlan 'Microsoft.Web/serverfarms@2020-10-01' = {
  name: parAppServicePlanName
  location: parLocation
  tags: parTags

  sku: {
    name: 'D1'
    tier: 'Shared'
  }
}

output outAppServicePlanId string = appServicePlan.id
output outAppServicePlanName string = appServicePlan.name
