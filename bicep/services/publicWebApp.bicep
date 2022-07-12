targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppInsightsName string

param parConnectivitySubscriptionId string
param parFrontDoorResourceGroupName string
param parDnsResourceGroupName string
param parFrontDoorName string
param parParentDnsName string

param parStrategicServicesSubscriptionId string
param parApiManagementResourceGroupName string
param parApiManagementName string
param parWebAppsResourceGroupName string
param parAppServicePlanName string

param parTags object

// Variables
var varWebAppName = 'webapp-geolocation-public-${parEnvironment}-${parLocation}'
var varWorkloadName = 'webapp-geolocation-public-${parEnvironment}'

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
    parApiManagementResourceGroupName: parApiManagementResourceGroupName
    parApiManagementName: parApiManagementName
    parAppServicePlanName: parAppServicePlanName
    parWorkloadSubscriptionId: subscription().subscriptionId
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
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parApiManagementName: parApiManagementName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parWorkloadName: varWebAppName
    parKeyVaultName: parKeyVaultName
    parTags: parTags
  }
}

module frontDoorEndpoint './../modules/frontDoorEndpoint.bicep' = {
  name: 'publicWebAppFrontDoorEndpoint'
  scope: resourceGroup(parConnectivitySubscriptionId, parFrontDoorResourceGroupName)

  params: {
    parFrontDoorName: parFrontDoorName
    parParentDnsName: parParentDnsName
    parDnsResourceGroupName: parDnsResourceGroupName
    parWorkloadName: varWorkloadName
    parOriginHostName: webApp.outputs.outWebAppDefaultHostName

    parTags: parTags
  }
}
