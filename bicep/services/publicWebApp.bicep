targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppInsightsName string
param parApiManagementName string
param parConnectivitySubscriptionId string
param parDnsResourceGroupName string
param parParentDnsName string
param parStrategicServicesSubscriptionId string
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

resource apiManagement 'Microsoft.ApiManagement/service@2021-08-01' existing = {
  name: parApiManagementName
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

resource apiManagementSubscription 'Microsoft.ApiManagement/service/subscriptions@2021-08-01' = {
  name: '${apiManagement.name}-${varWebAppName}-subscription'
  parent: apiManagement

  properties: {
    allowTracing: false
    displayName: varWebAppName
    scope: '/apis'
  }
}

resource webAppApiMgmtKey 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: '${apiManagement.name}-${varWebAppName}-apikey'
  parent: keyVault
  tags: parTags

  properties: {
    contentType: 'text/plain'
    value: apiManagementSubscription.properties.primaryKey
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
