targetScope = 'subscription'

// Parameters
param parLocation string
param parEnvironment string
param parLogWorkspaceName string

// Variables
var varResourceGroupName = 'rg-geolocation-${parEnvironment}-${parLocation}'
var varKeyVaultName = 'kv-geoloc-${parEnvironment}-${parLocation}'
var varAppInsightsName = 'ai-geolocation-${parEnvironment}-${parLocation}'
var varApimName = 'apim-geolocation-${parEnvironment}-${parLocation}'
var varAppServicePlanName = 'plan-geolocation-${parEnvironment}-${parLocation}'

resource defaultResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: varResourceGroupName
  location: parLocation
  properties: {}
}

// Platform
module keyVault 'platform/keyVault.bicep' = {
  name: 'keyVault'
  scope: resourceGroup(defaultResourceGroup.name)
  params: {
    parKeyVaultName: varKeyVaultName
    parLocation: parLocation
  }
}

module logging 'platform/logging.bicep' = {
  name: 'logging'
  scope: resourceGroup(defaultResourceGroup.name)
  params: {
    parLogWorkspaceName: parLogWorkspaceName
    parAppInsightsName: varAppInsightsName
    parKeyVaultName: keyVault.outputs.outKeyVaultName
    parLocation: parLocation
  }
}

module apiManagment 'platform/apiManagement.bicep' = {
  name: 'apiManagement'
  scope: resourceGroup(defaultResourceGroup.name)
  params: {
    parApimName: varApimName
    parAppInsightsName: logging.outputs.outAppInsightsName
    parKeyVaultName: keyVault.outputs.outKeyVaultName
    parLocation: parLocation
  }
}

module appServicePlan 'platform/appServicePlan.bicep' = {
  name: 'appServicePlan'
  scope: resourceGroup(defaultResourceGroup.name)
  params: {
    parAppServicePlanName: varAppServicePlanName
    parLocation: parLocation
  }
}
