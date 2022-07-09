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
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: parKeyVaultName
}

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

module apiManagementSubscription './../modules/apiManagementSubscription.bicep' = {
  name: '${parApiManagementName}-publicwebapp-subscription'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApimResourceGroupName)

  params: {
    parApiManagementName: parApiManagementName
    parWorkloadName: varWebAppName
  }
}

module webAppApiMgmtKey './../modules/keyVaultSecret.bicep' = {
  name: '${parApiManagementName}-publicwebapp-subscription'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApimResourceGroupName)

  params: {
    parKeyVaultName: parKeyVaultName
    parSecretName: '${parApiManagementName}-${varWebAppName}-apikey'
    parSecretValue: apiManagementSubscription.outputs.outApiManagementSubcriptionKey
    parTags: parTags
  }
}

module webAppKeyVaultPermissions './../modules/keyVaultAccessPolicy.bicep' = {
  name: '${varWebAppName}-${keyVault.name}'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: webApp.outputs.outWebAppIdentityPrincipalId
  }
}
