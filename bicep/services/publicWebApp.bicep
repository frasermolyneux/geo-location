targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppInsightsName string
param parConnectivitySubscriptionId string
param parDnsResourceGroupName string
param parParentDnsName string
param parStrategicServicesSubscriptionId string
param parApimResourceGroupName string
param parApiManagementName string
param parWebAppsResourceGroupName string
param parAppServicePlanName string
param parTags object

// Variables
var varWebAppName = 'webapp-geolocation-public-${parEnvironment}-${parLocation}'
var varFrontDoorName = 'fd-webapp-geolocation-public-${parEnvironment}'
var varFrontDoorDns = 'webapp-geolocation-public-${parEnvironment}'

// Existing Resources
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: parKeyVaultName
}

// Module Resources
module scopedPublicWebApp 'modules/scopedPublicWebApp.bicep' = {
  name: 'scopedPublicWebApp'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parWebAppsResourceGroupName)

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: parKeyVaultName
    parAppInsightsName: parAppInsightsName
    parApiManagementName: parApiManagementName
    parAppServicePlanName: parAppServicePlanName
    parWorkloadSubscriptionId: subscription().id
    parWorkloadResourceGroupName: resourceGroup().name
    parTags: parTags
  }
}

module apiManagementSubscription 'modules/apimSubscription.bicep' = {
  name: '${parApiManagementName}-${varWebAppName}-subscription'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApimResourceGroupName)

  params: {
    parApiManagementName: parApiManagementName
    parWorkloadName: varWebAppName
  }
}

resource webAppApiMgmtKey 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: '${parApiManagementName}-${varWebAppName}-apikey'
  parent: keyVault
  tags: parTags

  properties: {
    contentType: 'text/plain'
    value: apiManagementSubscription.outputs.outApiManagementSubcriptionKey
  }
}

resource webAppKeyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2021-11-01-preview' = {
  name: 'add'
  parent: keyVault

  properties: {
    accessPolicies: [
      {
        objectId: scopedPublicWebApp.outputs.outWebAppIdentityPrincipalId
        permissions: {
          certificates: []
          keys: []
          secrets: [
            'get'
          ]
          storage: []
        }
        tenantId: tenant().tenantId
      }
    ]
  }
}

module publicWebAppFrontDoor 'modules/frontDoor.bicep' = {
  name: 'publicWebAppFrontDoor'

  params: {
    parFrontDoorName: varFrontDoorName
    parFrontDoorDns: varFrontDoorDns
    parParentDnsName: parParentDnsName
    parConnectivitySubscriptionId: parConnectivitySubscriptionId
    parDnsResourceGroupName: parDnsResourceGroupName
    parOriginHostName: scopedPublicWebApp.outputs.outWebAppDefaultHostName
    parTags: parTags
  }
}
