targetScope = 'resourceGroup'

param parLocation string
param parEnvironment string

// Variables
var varKeyVaultName = 'kv-geoloc-${parEnvironment}-${parLocation}'
var varAppInsightsName = 'ai-geolocation-${parEnvironment}-${parLocation}'
var varApimName = 'apim-geolocation-${parEnvironment}-${parLocation}'
var varAppServicePlanName = 'plan-geolocation-${parEnvironment}-${parLocation}'

module lookupWebApp 'services/lookupWebApp.bicep' = {
  name: 'lookupWebApp'
  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppServicePlanName: varAppServicePlanName
    parAppInsightsName: varAppInsightsName
    parApiManagementName: varApimName
  }
}

module publicWebApp 'services/publicWebApp.bicep' = {
  name: 'publicWebApp'
  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppServicePlanName: varAppServicePlanName
    parAppInsightsName: varAppInsightsName
    parApiManagementName: varApimName
  }
}
