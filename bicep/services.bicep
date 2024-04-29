targetScope = 'resourceGroup'

@description('The environment name (e.g. dev, test, prod)')
param parEnvironment string

@description('The location of the resource group')
param parLocation string

@description('The instance name (e.g. 01, 02, 03, etc.')
param parInstance string

@description('The logging workspace details (subscriptionId, resourceGroupName, workspaceName)')
param parLogging object

@description('The Front Door configuration')
param parFrontDoor object

@description('The DNS configuration')
param parDns object

@description('The Strategic Services configuration')
param parStrategicServices object

@description('The tags to apply to the resources')
param parTags object

@description('The key vault create mode (recover, default)')
param parKeyVaultCreateMode string = 'recover'

// Variables
var varEnvironmentUniqueId = uniqueString('geolocation', parEnvironment, parInstance)

var varKeyVaultName = 'kv-${varEnvironmentUniqueId}-${parLocation}'
var varAppServicePlanName = 'plan-geolocation-${parEnvironment}-${parLocation}-${parInstance}'
var varApiManagementName = 'apim-geolocation-${parEnvironment}-${parLocation}-${varEnvironmentUniqueId}'
var varAppInsightsName = 'ai-geolocation-${parEnvironment}-${parLocation}-${parInstance}'

// Module Resources
module keyVault 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvault:latest' = {
  name: '${deployment().name}-keyvault'

  params: {
    parKeyVaultName: varKeyVaultName
    parLocation: parLocation

    parKeyVaultCreateMode: parKeyVaultCreateMode

    parEnabledForRbacAuthorization: true

    parTags: parTags
  }
}

module appInsights 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/appinsights:latest' = {
  name: '${deployment().name}-appInsights'

  params: {
    parAppInsightsName: varAppInsightsName
    parLocation: parLocation
    parLoggingSubscriptionId: parLogging.SubscriptionId
    parLoggingResourceGroupName: parLogging.WorkspaceResourceGroupName
    parLoggingWorkspaceName: parLogging.WorkspaceName
    parTags: parTags
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: varAppServicePlanName
  location: parLocation

  sku: {
    name: 'B1'
    tier: 'Basic'
  }

  kind: 'linux'
  tags: parTags
}

resource apiManagement 'Microsoft.ApiManagement/service@2023-05-01-preview' = {
  name: varApiManagementName
  location: parLocation

  sku: {
    capacity: 0
    name: 'Consumption'
  }
  properties: {
    publisherEmail: 'admin@molyneux.io'
    publisherName: 'Molyneux.IO'
  }
}

@description('https://learn.microsoft.com/en-gb/azure/role-based-access-control/built-in-roles#key-vault-secrets-user')
resource keyVaultSecretUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '4633458b-17de-408a-b874-0445c86b69e6'
}

module keyVaultSecretUserRoleAssignmentApim 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvaultroleassignment:latest' = {
  name: '${deployment().name}-kvapimrole'

  params: {
    parKeyVaultName: keyVault.outputs.outKeyVaultName
    parRoleDefinitionId: keyVaultSecretUserRoleDefinition.id
    parPrincipalId: apiManagement.identity.principalId
  }
}

module apiManagementLogger 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/apimanagementlogger:latest' = {
  name: '${deployment().name}-apimlogger'

  params: {
    parApiManagementName: apiManagement.name

    parAppInsightsRef: {
      Name: varAppInsightsName
      SubscriptionId: subscription().subscriptionId
      ResourceGroupName: resourceGroup().name
    }
  }
}

module lookupWebApi 'services/lookupWebApi.bicep' = {
  name: '${deployment().name}-webapi'
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
  name: '${deployment().name}-webapp'
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
output outKeyVaultName string = keyVault.outputs.outKeyVaultName
output outWebAppIdentityPrincipalId string = publicWebApp.outputs.outWebAppIdentityPrincipalId
output outWebAppName string = publicWebApp.outputs.outWebAppName
output outWebApiName string = lookupWebApi.outputs.outWebAppName
