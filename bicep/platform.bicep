targetScope = 'subscription'

@description('The environment name (e.g. dev, test, prod)')
param parEnvironment string

@description('The location of the resource group and resources')
param parLocation string

@description('The instance name (e.g. 01, 02, 03, etc.)')
param parInstance string

@description('The logging workspace details (subscriptionId, resourceGroupName, workspaceName)')
param parLogging object

@description('The strategic services details (subscriptionId, resourceGroupName, apiManagementName)')
param parStrategicServices object

@description('The tags to apply to the resources')
param parTags object

@description('The key vault create mode (recover, default)')
param parKeyVaultCreateMode string = 'recover'

// Variables
var varEnvironmentUniqueId = uniqueString('geolocation', parEnvironment, parInstance)

var varResourceGroupName = 'rg-geolocation-${parEnvironment}-${parLocation}-${parInstance}'
var varKeyVaultName = 'kv-${varEnvironmentUniqueId}-${parLocation}'
var varAppInsightsName = 'ai-geolocation-${parEnvironment}-${parLocation}-${parInstance}'

// Existing Out-Of-Scope Resources
resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: parStrategicServices.ApiManagementName
  scope: resourceGroup(parStrategicServices.SubscriptionId, parStrategicServices.ApiManagementResourceGroupName)
}

// Module Resources
resource defaultResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: varResourceGroupName
  location: parLocation
  tags: parTags

  properties: {}
}

module keyVault 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvault:latest' = {
  name: '${deployment().name}-keyvault'
  scope: resourceGroup(defaultResourceGroup.name)
  params: {
    parKeyVaultName: varKeyVaultName
    parLocation: parLocation

    parKeyVaultCreateMode: parKeyVaultCreateMode

    parEnabledForRbacAuthorization: true

    parTags: parTags
  }
}

@description('https://learn.microsoft.com/en-gb/azure/role-based-access-control/built-in-roles#key-vault-secrets-user')
resource keyVaultSecretUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '4633458b-17de-408a-b874-0445c86b69e6'
}

module keyVaultSecretUserRoleAssignmentApim 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvaultroleassignment:latest' = {
  name: '${deployment().name}-kvapimrole'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    parKeyVaultName: keyVault.outputs.outKeyVaultName
    parRoleDefinitionId: keyVaultSecretUserRoleDefinition.id
    parPrincipalId: apiManagement.identity.principalId
  }
}

module appInsights 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/appinsights:latest' = {
  name: '${deployment().name}-appInsights'
  scope: resourceGroup(defaultResourceGroup.name)
  params: {
    parAppInsightsName: varAppInsightsName
    parKeyVaultName: keyVault.outputs.outKeyVaultName
    parLocation: parLocation
    parLoggingSubscriptionId: parLogging.SubscriptionId
    parLoggingResourceGroupName: parLogging.WorkspaceResourceGroupName
    parLoggingWorkspaceName: parLogging.WorkspaceName
    parTags: parTags
  }
}

module apiManagementLogger 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/apimanagementlogger:latest' = {
  name: '${deployment().name}-apimlogger'
  scope: resourceGroup(parStrategicServices.SubscriptionId, parStrategicServices.ApiManagementResourceGroupName)

  params: {
    parApiManagementName: parStrategicServices.ApiManagementName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: defaultResourceGroup.name
    parAppInsightsName: appInsights.outputs.outAppInsightsName
    parKeyVaultName: keyVault.outputs.outKeyVaultName
  }

  dependsOn: [
    keyVaultSecretUserRoleAssignmentApim
  ]
}

// Outputs
output keyVaultName string = keyVault.outputs.outKeyVaultName
