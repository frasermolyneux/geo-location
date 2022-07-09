targetScope = 'subscription'

// Parameters
param parLocation string
param parEnvironment string
param parLoggingSubscriptionId string
param parLoggingResourceGroupName string
param parLoggingWorkspaceName string
param parStrategicServicesSubscriptionId string
param parApimResourceGroupName string
param parApiManagementName string
param parTags object

// Variables
var varResourceGroupName = 'rg-geolocation-${parEnvironment}-${parLocation}'
var varKeyVaultName = 'kv-geoloc-${parEnvironment}-${parLocation}'
var varAppInsightsName = 'ai-geolocation-${parEnvironment}-${parLocation}'

// Existing Resources
resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: parApiManagementName
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApimResourceGroupName)
}

// Module Resources
resource defaultResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: varResourceGroupName
  location: parLocation
  tags: parTags

  properties: {}
}

module keyVault 'modules/keyVault.bicep' = {
  name: 'keyVault'
  scope: resourceGroup(defaultResourceGroup.name)
  params: {
    parKeyVaultName: varKeyVaultName
    parLocation: parLocation
    parTags: parTags
  }
}

module apiManagementKeyVaultPermissions 'modules/keyVaultAccessPolicy.bicep' = {
  name: '${apiManagement.name}-${keyVault.name}'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    parKeyVaultName: keyVault.outputs.outKeyVaultName
    parPrincipalId: apiManagement.identity.principalId
  }
}

module logging 'platform/logging.bicep' = {
  name: 'logging'
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

module apiManagementLogger 'modules/apiManagementLogger.bicep' = {
  name: '${apiManagement.name}-${varAppInsightsName}'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApimResourceGroupName)

  params: {
    parApiManagementName: parApiManagementName
    parAppInsightsInstrumentationKeySecretName: logging.outputs.outAppInsightsInstrumentationKeySecretName
    parKeyVaultUri: keyVault.outputs.outKeyVaultUri
    parAppInsightsName: logging.outputs.outAppInsightsName
    parAppInsightsId: logging.outputs.outAppInsightsId
  }
}
