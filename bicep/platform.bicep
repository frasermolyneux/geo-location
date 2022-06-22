targetScope = 'subscription'

// Parameters
param parLocation string
param parEnvironment string

// Variables
var varResourceGroupName = 'rg-geolocation-${parEnvironment}-${parLocation}'
var varAppServicePlanName = 'plan-geolocation-${parEnvironment}-${parLocation}'

resource defaultResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: varResourceGroupName
  location: parLocation
  properties: {}
}

// Platform
module appServicePlan 'platform/appServicePlan.bicep' = {
  name: 'appServicePlan'
  scope: resourceGroup(defaultResourceGroup.name)
  params: {
    parAppServicePlanName: varAppServicePlanName
    parLocation: parLocation
  }
}
