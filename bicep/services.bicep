targetScope = 'resourceGroup'

// Parameters
param parEnvironment string
param parLocation string
param parInstance string

param parFrontDoor object
param parDns object
param parStrategicServices object

param parTags object

// Variables
var varEnvironmentUniqueId = uniqueString('geolocation', parEnvironment, parInstance)
var varDeploymentPrefix = 'services-${varEnvironmentUniqueId}' //Prevent deployment naming conflicts

var varKeyVaultName = 'kv-${varEnvironmentUniqueId}-${parLocation}'
var varAppInsightsName = 'ai-geolocation-${parEnvironment}-${parLocation}-${parInstance}'

module lookupWebApi 'services/lookupWebApi.bicep' = {
  name: '${varDeploymentPrefix}-lookupWebApi'
  params: {
    parEnvironment: parEnvironment
    parEnvironmentUniqueId: varEnvironmentUniqueId
    parLocation: parLocation
    parInstance: parInstance

    parKeyVaultName: varKeyVaultName
    parAppInsightsName: varAppInsightsName
    parFrontDoorSubscriptionId: parFrontDoor.SubscriptionId
    parDnsSubscriptionId: parDns.SubscriptionId
    parFrontDoorResourceGroupName: parFrontDoor.FrontDoorResourceGroupName
    parDnsResourceGroupName: parDns.DnsResourceGroupName
    parFrontDoorName: parFrontDoor.FrontDoorName
    parParentDnsName: parDns.ParentDnsName
    parStrategicServicesSubscriptionId: parStrategicServices.SubscriptionId
    parApiManagementResourceGroupName: parStrategicServices.ApiManagementResourceGroupName
    parApiManagementName: parStrategicServices.ApiManagementName
    parWebAppsResourceGroupName: parStrategicServices.WebAppsResourceGroupName
    parAppServicePlanName: parStrategicServices.AppServicePlanName
    parTags: parTags
  }
}

module publicWebApp 'services/publicWebApp.bicep' = {
  name: '${varDeploymentPrefix}-publicWebApp'
  params: {
    parEnvironment: parEnvironment
    parEnvironmentUniqueId: varEnvironmentUniqueId
    parLocation: parLocation
    parInstance: parInstance

    parKeyVaultName: varKeyVaultName
    parAppInsightsName: varAppInsightsName
    parFrontDoorSubscriptionId: parFrontDoor.SubscriptionId
    parDnsSubscriptionId: parDns.SubscriptionId
    parFrontDoorResourceGroupName: parFrontDoor.FrontDoorResourceGroupName
    parDnsResourceGroupName: parDns.DnsResourceGroupName
    parFrontDoorName: parFrontDoor.FrontDoorName
    parPublicWebAppDnsPrefix: parDns.PublicWebAppDnsPrefix
    parParentDnsName: parDns.ParentDnsName
    parStrategicServicesSubscriptionId: parStrategicServices.SubscriptionId
    parApiManagementResourceGroupName: parStrategicServices.ApiManagementResourceGroupName
    parApiManagementName: parStrategicServices.ApiManagementName
    parWebAppsResourceGroupName: parStrategicServices.WebAppsResourceGroupName
    parAppServicePlanName: parStrategicServices.AppServicePlanName
    parTags: parTags
  }
}

// Outputs
output outWebAppIdentityPrincipalId string = publicWebApp.outputs.outWebAppIdentityPrincipalId
