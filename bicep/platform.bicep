targetScope = 'subscription'

// Parameters
param parLocation string
param parEnvironment string

param parLoggingSubscriptionId string
param parLoggingResourceGroupName string
param parLoggingWorkspaceName string

param parStrategicServicesSubscriptionId string
param parApiManagementResourceGroupName string
param parApiManagementName string

param parKeyVaultCreateMode string = 'recover'

param parDeployPrincipalId string

param parTags object

// Variables
var varDeploymentPrefix = 'geolocationPlatform' //Prevent deployment naming conflicts
var varResourceGroupName = 'rg-geolocation-${parEnvironment}-${parLocation}'
var varKeyVaultName = 'kv-geoloc-${parEnvironment}-${parLocation}'
var varAppInsightsName = 'ai-geolocation-${parEnvironment}-${parLocation}'

// Existing Out-Of-Scope Resources
resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: parApiManagementName
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)
}

// Module Resources
resource defaultResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: varResourceGroupName
  location: parLocation
  tags: parTags

  properties: {}
}

module keyVault 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/keyvault:latest' = {
  name: '${varDeploymentPrefix}-keyVault'
  scope: resourceGroup(defaultResourceGroup.name)
  params: {
    parKeyVaultName: varKeyVaultName
    parLocation: parLocation

    parKeyVaultCreateMode: parKeyVaultCreateMode

    parTags: parTags
  }
}

module deployKeyVaultAccessPolicy 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/keyvaultaccesspolicy:latest' = {
  name: '${varDeploymentPrefix}-deployKeyVaultAccessPolicy'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    parKeyVaultName: keyVault.outputs.outKeyVaultName
    parPrincipalId: parDeployPrincipalId
    parSecretsPermissions: [ 'get', 'set' ]
  }
}

module apimKeyVaultAccessPolicy 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/keyvaultaccesspolicy:latest' = {
  name: '${varDeploymentPrefix}-apimKeyVaultAccessPolicy'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    parKeyVaultName: keyVault.outputs.outKeyVaultName
    parPrincipalId: apiManagement.identity.principalId
    parSecretsPermissions: [ 'get' ]
  }
}

module appInsights 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/appinsights:latest' = {
  name: '${varDeploymentPrefix}-appInsights'
  scope: resourceGroup(defaultResourceGroup.name)
  params: {
    parAppInsightsName: varAppInsightsName
    parKeyVaultName: keyVault.outputs.outKeyVaultName
    parLocation: parLocation
    parLoggingSubscriptionId: parLoggingSubscriptionId
    parLoggingResourceGroupName: parLoggingResourceGroupName
    parLoggingWorkspaceName: parLoggingWorkspaceName
    parTags: parTags
  }
}

module apiManagementLogger 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/apimanagementlogger:latest' = {
  name: '${varDeploymentPrefix}-apiManagementLogger'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parApiManagementName: parApiManagementName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: defaultResourceGroup.name
    parAppInsightsName: appInsights.outputs.outAppInsightsName
    parKeyVaultName: keyVault.outputs.outKeyVaultName
  }
}
