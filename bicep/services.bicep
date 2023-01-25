targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string

param parFrontDoorSubscriptionId string
param parFrontDoorResourceGroupName string
param parFrontDoorName string

param parDnsSubscriptionId string
param parDnsResourceGroupName string
param parPublicWebAppDnsPrefix string
param parParentDnsName string

param parStrategicServicesSubscriptionId string
param parApiManagementResourceGroupName string
param parApiManagementName string
param parWebAppsResourceGroupName string
param parAppServicePlanName string

param parTags object

// Variables
var environmentUniqueId = uniqueString('geolocation', parEnvironment)
var varDeploymentPrefix = 'services-${environmentUniqueId}' //Prevent deployment naming conflicts

var varKeyVaultName = 'kv-${environmentUniqueId}-${parLocation}'
var varAppInsightsName = 'ai-geolocation-${environmentUniqueId}-${parEnvironment}-${parLocation}'

module lookupWebApi 'services/lookupWebApi.bicep' = {
  name: '${varDeploymentPrefix}-lookupWebApi'
  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppInsightsName: varAppInsightsName
    parFrontDoorSubscriptionId: parFrontDoorSubscriptionId
    parDnsSubscriptionId: parDnsSubscriptionId
    parFrontDoorResourceGroupName: parFrontDoorResourceGroupName
    parDnsResourceGroupName: parDnsResourceGroupName
    parFrontDoorName: parFrontDoorName
    parParentDnsName: parParentDnsName
    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parApiManagementResourceGroupName: parApiManagementResourceGroupName
    parApiManagementName: parApiManagementName
    parWebAppsResourceGroupName: parWebAppsResourceGroupName
    parAppServicePlanName: parAppServicePlanName
    parTags: parTags
  }
}

module publicWebApp 'services/publicWebApp.bicep' = {
  name: '${varDeploymentPrefix}-publicWebApp'
  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppInsightsName: varAppInsightsName
    parFrontDoorSubscriptionId: parFrontDoorSubscriptionId
    parDnsSubscriptionId: parDnsSubscriptionId
    parFrontDoorResourceGroupName: parFrontDoorResourceGroupName
    parDnsResourceGroupName: parDnsResourceGroupName
    parFrontDoorName: parFrontDoorName
    parPublicWebAppDnsPrefix: parPublicWebAppDnsPrefix
    parParentDnsName: parParentDnsName
    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parApiManagementResourceGroupName: parApiManagementResourceGroupName
    parApiManagementName: parApiManagementName
    parWebAppsResourceGroupName: parWebAppsResourceGroupName
    parAppServicePlanName: parAppServicePlanName
    parTags: parTags
  }
}
