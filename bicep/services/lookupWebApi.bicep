targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppInsightsName string
param parParentDnsName string
param parStrategicServicesSubscriptionId string
param parApimResourceGroupName string
param parApiManagementName string
param parWebAppsResourceGroupName string
param parAppServicePlanName string
param parTags object

// Variables
var varFrontDoorDns = 'webapi-geolocation-lookup-${parEnvironment}'

// Existing Resources
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: parKeyVaultName
}

// Module Resources
module webApp 'lookupWebApi/webApp.bicep' = {
  name: 'lookupWebApi'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parWebAppsResourceGroupName)

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: parKeyVaultName
    parAppInsightsName: parAppInsightsName
    parAppServicePlanName: parAppServicePlanName
    parWorkloadSubscriptionId: subscription().id
    parWorkloadResourceGroupName: resourceGroup().name
    parTags: parTags
  }
}

module webAppKeyVaultAccessPolicy './../modules/keyVaultAccessPolicy.bicep' = {
  name: 'lookupWebApiKeyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: webApp.outputs.outWebAppIdentityPrincipalId
  }
}

module apiManagementLookupApi 'lookupWebApi/apiManagementApi.bicep' = {
  name: 'apiManagementLookupApi'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApimResourceGroupName)

  params: {
    parApiManagementName: parApiManagementName
    parFrontDoorDns: varFrontDoorDns
    parParentDnsName: parParentDnsName
    parEnvironment: parEnvironment
    parKeyVaultUri: keyVault.properties.vaultUri
    parAppInsightsName: parAppInsightsName
  }
}
