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

// Module Resources
module webApp 'publicWebApp/webApp.bicep' = {
  name: 'publicWebApp'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parWebAppsResourceGroupName)

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: parKeyVaultName
    parAppInsightsName: parAppInsightsName
    parApiManagementSubscriptionId: parStrategicServicesSubscriptionId
    parApiManagementResourceGroupName: parApimResourceGroupName
    parApiManagementName: parApiManagementName
    parAppServicePlanName: parAppServicePlanName
    parWorkloadSubscriptionId: subscription().id
    parWorkloadResourceGroupName: resourceGroup().name
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

module apiMgmtSubscriptionKeyVaultSecret './../modules/apiManagementSubscriptionKeyVaultSecret.bicep' = {
  name: 'publicWebAppApiMgmtSubscriptionKeyVaultSecret'

  params: {
    parKeyVaultName: parKeyVaultName
    parApiManagementSubscriptionName: apiManagementSubscription.outputs.outSubscriptionName
    parApiManagementSubscriptionId: parStrategicServicesSubscriptionId
    parApiManagementResourceGroupName: parApimResourceGroupName
    parApiManagementName: parApiManagementName
    parTags: parTags
  }
}
