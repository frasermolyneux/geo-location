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

@description('https://learn.microsoft.com/en-gb/azure/role-based-access-control/built-in-roles#key-vault-secrets-user')
resource keyVaultSecretUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '4633458b-17de-408a-b874-0445c86b69e6'
}

module lookupWebApiKeyVaultRoleAssignment 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvaultroleassignment:latest' = {
  name: '${varDeploymentPrefix}-lookupWebApiKeyVaultRoleAssignment'

  params: {
    parKeyVaultName: varKeyVaultName
    parRoleDefinitionId: keyVaultSecretUserRoleDefinition.id
    parPrincipalId: lookupWebApi.outputs.outWebAppIdentityPrincipalId
  }
}

module publicWebAppKeyVaultRoleAssignment 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvaultroleassignment:latest' = {
  name: '${varDeploymentPrefix}-publicWebAppKeyVaultRoleAssignment'

  params: {
    parKeyVaultName: varKeyVaultName
    parRoleDefinitionId: keyVaultSecretUserRoleDefinition.id
    parPrincipalId: publicWebApp.outputs.outWebAppIdentityPrincipalId
  }
}

// Outputs
output outWebAppIdentityPrincipalId string = publicWebApp.outputs.outWebAppIdentityPrincipalId
output outWebAppName string = publicWebApp.outputs.outWebAppName
output outWebApiName string = lookupWebApi.outputs.outWebAppName
