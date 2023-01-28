targetScope = 'resourceGroup'

// Parameters
param parEnvironment string
param parEnvironmentUniqueId string
param parLocation string
param parInstance string

param parKeyVaultName string
param parAppInsightsName string

param parFrontDoorSubscriptionId string
param parFrontDoorResourceGroupName string
param parFrontDoorName string

param parDnsSubscriptionId string
param parDnsResourceGroupName string
param parParentDnsName string

param parStrategicServicesSubscriptionId string
param parApiManagementResourceGroupName string
param parApiManagementName string
param parWebAppsResourceGroupName string
param parAppServicePlanName string

param parTags object

// Variables
var varDeploymentPrefix = 'api-${parEnvironmentUniqueId}' //Prevent deployment naming conflicts

var varWorkloadName = 'app-geolocation-api-${parEnvironment}-${parInstance}-${parEnvironmentUniqueId}'

// Module Resources
module appDataStorage 'lookupWebApi/appDataStorage.bicep' = {
  name: '${varDeploymentPrefix}-appDataStorage'

  params: {
    parDeploymentPrefix: varDeploymentPrefix
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: parKeyVaultName
    parTags: parTags
  }
}

module webApp 'lookupWebApi/webApp.bicep' = {
  name: '${varDeploymentPrefix}-webApp'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parWebAppsResourceGroupName)

  params: {
    parEnvironment: parEnvironment
    parEnvironmentUniqueId: parEnvironmentUniqueId
    parLocation: parLocation
    parInstance: parInstance

    parKeyVaultName: parKeyVaultName
    parAppInsightsName: parAppInsightsName
    parAppDataStorageAccountName: appDataStorage.outputs.outStorageAccountName
    parAppServicePlanName: parAppServicePlanName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parTags: parTags
  }
}

module keyVaultAccessPolicy 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvaultaccesspolicy:latest' = {
  name: '${varDeploymentPrefix}-keyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: webApp.outputs.outWebAppIdentityPrincipalId
  }
}

module apiManagementApi 'lookupWebApi/apiManagementApi.bicep' = {
  name: '${varDeploymentPrefix}-apiManagementApi'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parApiManagementName: parApiManagementName
    parFrontDoorDns: varWorkloadName
    parParentDnsName: parParentDnsName
    parEnvironment: parEnvironment
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parKeyVaultName: parKeyVaultName
    parAppInsightsName: parAppInsightsName
  }
}

module frontDoorEndpoint 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/frontdoorendpoint:latest' = {
  name: '${varDeploymentPrefix}-frontDoorEndpoint'
  scope: resourceGroup(parFrontDoorSubscriptionId, parFrontDoorResourceGroupName)

  params: {
    parDeploymentPrefix: varDeploymentPrefix
    parFrontDoorName: parFrontDoorName
    parDnsSubscriptionId: parDnsSubscriptionId
    parDnsResourceGroupName: parDnsResourceGroupName
    parParentDnsName: parParentDnsName
    parWorkloadName: varWorkloadName
    parOriginHostName: webApp.outputs.outWebAppDefaultHostName
    parDnsZoneHostnamePrefix: varWorkloadName
    parCustomHostname: '${varWorkloadName}.${parParentDnsName}'
    parTags: parTags
  }
}
