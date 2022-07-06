targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppServicePlanName string
param parAppInsightsName string
param parApiManagementName string
param parManagementSubscriptionId string
param parDnsResourceGroupName string
param parParentDnsName string
param parTags object

// Variables
var varWebAppName = 'webapp-geolocation-public-${parEnvironment}-${parLocation}'
var varFrontDoorName = 'fd-webapp-geolocation-public-${parEnvironment}'
var varFrontDoorDns = 'webapp-geolocation-public-${parEnvironment}'

// Existing Resources
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: parKeyVaultName
}

resource appServicePlan 'Microsoft.Web/serverfarms@2020-10-01' existing = {
  name: parAppServicePlanName
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: parAppInsightsName
}

resource apiManagement 'Microsoft.ApiManagement/service@2021-08-01' existing = {
  name: parApiManagementName
}

// Module Resources
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

resource webApp 'Microsoft.Web/sites@2020-06-01' = {
  name: varWebAppName
  location: parLocation
  kind: 'app'
  tags: parTags

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    serverFarmId: appServicePlan.id

    httpsOnly: true

    siteConfig: {
      ftpsState: 'Disabled'

      netFrameworkVersion: 'v6.0'
      minTlsVersion: '1.2'

      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${appInsights.name}-instrumentationkey)'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${appInsights.name}-connectionstring)'
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~2'
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'apim-base-url'
          value: apiManagement.properties.gatewayUrl
        }
        {
          name: 'apim-subscription-key'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${apiManagement.name}-${varWebAppName}-apikey)'
        }
      ]
    }
  }
}

resource webAppKeyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2021-11-01-preview' = {
  name: 'add'
  parent: keyVault

  properties: {
    accessPolicies: [
      {
        objectId: webApp.identity.principalId
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

module frontDoor 'modules/frontDoor.bicep' = {
  name: 'dnsZone'

  params: {
    parFrontDoorName: varFrontDoorName
    parFrontDoorDns: varFrontDoorDns
    parParentDnsName: parParentDnsName
    parManagementSubscriptionId: parManagementSubscriptionId
    parDnsResourceGroupName: parDnsResourceGroupName
    parOriginHostName: webApp.properties.defaultHostName
    parTags: parTags
  }
}
