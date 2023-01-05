targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string

param parKeyVaultName string
param parAppInsightsName string
param parAppDataStorageAccountName string

param parAppServicePlanName string

param parWorkloadSubscriptionId string
param parWorkloadResourceGroupName string

param parTags object

// Variables
var varWebAppName = 'webapi-geolocation-lookup-${parEnvironment}-${parLocation}'

// Existing In-Scope Resources
resource appServicePlan 'Microsoft.Web/serverfarms@2020-10-01' existing = {
  name: parAppServicePlanName
}

// Existing Out-Of-Scope Resources
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: parKeyVaultName
  scope: resourceGroup(parWorkloadSubscriptionId, parWorkloadResourceGroupName)
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: parAppInsightsName
  scope: resourceGroup(parWorkloadSubscriptionId, parWorkloadResourceGroupName)
}

resource appDataStorageAccount 'Microsoft.Storage/storageAccounts@2019-06-01' existing = {
  name: parAppDataStorageAccountName
  scope: resourceGroup(parWorkloadSubscriptionId, parWorkloadResourceGroupName)
}

// Module Resources
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

      alwaysOn: true
      linuxFxVersion: 'DOTNETCORE|7.0'
      netFrameworkVersion: '7.0'
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
          value: '~3'
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
          name: 'AzureAd__TenantId'
          value: tenant().tenantId
        }
        {
          name: 'AzureAd__Instance'
          value: environment().authentication.loginEndpoint
        }
        {
          name: 'AzureAd__ClientId'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=geolocation-lookup-api-${parEnvironment}-clientid)'
        }
        {
          name: 'AzureAd__ClientSecret'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=geolocation-lookup-api-${parEnvironment}-clientsecret)'
        }
        {
          name: 'AzureAd__Audience'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=geolocation-lookup-api-${parEnvironment}-clientid)'
        }
        {
          name: 'maxmind_apikey'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=maxmind-apikey)'
        }
        {
          name: 'maxmind_userid'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=maxmind-userid)'
        }
        {
          name: 'appdata_storage_connectionstring'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${appDataStorageAccount.name}-connectionstring)'
        }
      ]
    }
  }
}

// Outputs
output outWebAppDefaultHostName string = webApp.properties.defaultHostName
output outWebAppIdentityPrincipalId string = webApp.identity.principalId
