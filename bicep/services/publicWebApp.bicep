targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppInsightsName string
param parStrategicServicesSubscriptionId string
param parApimResourceGroupName string
param parApiManagementName string
param parWebAppsResourceGroupName string
param parAppServicePlanName string
param parTags object

// Variables
var varWebAppName = 'webapp-geolocation-public-${parEnvironment}-${parLocation}'

// Existing Resources
resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: parApiManagementName
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApimResourceGroupName)
}

// Module Resources
module webApp 'publicWebApp/webApp.bicep' = {
  name: 'publicWebApp'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parWebAppsResourceGroupName)

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: parKeyVaultName
    parAppInsightsName: parAppInsightsName
    parApiManagementName: parApiManagementName
    parApiManagementGatewayUrl: apiManagement.properties.gatewayUrl
    parAppServicePlanName: parAppServicePlanName
    parTags: parTags
  }
}

module webAppKeyVaultAccessPolicy './../modules/keyVaultAccessPolicy.bicep' = {
  name: 'publicWebAppKeyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: webApp.outputs.outWebAppIdentityPrincipalId
  }
}

module apiManagementSubscription './../modules/apiManagementSubscription.bicep' = {
  name: 'publicWebAppApiManagementSubscription'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApimResourceGroupName)

  params: {
    parApiManagementName: parApiManagementName
    parWorkloadName: varWebAppName
  }
}

module apiMgmtSubscriptionKeyVaultSecret './../modules/keyVaultSecret.bicep' = {
  name: 'publicWebAppApiMgmtSubscriptionKeyVaultSecret'

  params: {
    parKeyVaultName: parKeyVaultName
    parSecretName: '${parApiManagementName}-${varWebAppName}-apikey'
    parSecretValue: apiManagementSubscription.outputs.outApiManagementSubcriptionKey
    parTags: parTags
  }
}
