targetScope = 'resourceGroup'

// Parameters
@description('The environment name (e.g. dev, test, prod)')
param parEnvironment string

@description('The location of the resource group.')
param parLocation string = resourceGroup().location

@description('The instance name (e.g. 01, 02, 03)')
param parInstance string

@description('The name of the key vault.')
param parKeyVaultRef object

@description('The app insights reference')
param parAppInsightsRef object

@description('The app service plan Ref')
param parAppServicePlanRef object

@description('The api management Ref')
param parApiManagementRef object

@description('The dns configuration object')
param parDns object

@description('The tags to apply to the resources.')
param parTags object = resourceGroup().tags

// Variables
var varEnvironmentUniqueId = uniqueString('app-geolocation-web', parEnvironment, parInstance)
var varWebAppName = 'app-geolocation-web-${parEnvironment}-${parLocation}-${parInstance}-${varEnvironmentUniqueId}'

// Existing Resources
@description('https://learn.microsoft.com/en-gb/azure/role-based-access-control/built-in-roles#key-vault-secrets-user')
resource keyVaultSecretUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '4633458b-17de-408a-b874-0445c86b69e6'
}

resource keyVault 'Microsoft.KeyVault/vaults@2021-06-01-preview' existing = {
  name: parKeyVaultRef.Name
  scope: resourceGroup(parKeyVaultRef.SubscriptionId, parKeyVaultRef.ResourceGroupName)
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: parAppInsightsRef.Name
  scope: resourceGroup(parAppInsightsRef.SubscriptionId, parAppInsightsRef.ResourceGroupName)
}

resource appServicePlan 'Microsoft.Web/serverfarms@2020-10-01' existing = {
  name: parAppServicePlanRef.Name
  scope: resourceGroup(parAppServicePlanRef.SubscriptionId, parAppServicePlanRef.ResourceGroupName)
}

resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: parApiManagementRef.Name
}

// Module Resources
module apiManagementSubscription 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/apimanagementsubscription:latest' = {
  name: '${deployment().name}-apimsubscription'

  params: {
    apiManagementName: apiManagement.name
    subscriptionName: varWebAppName
    apiScope: '/apis/geolocation-api'
    keyVaultName: keyVault.name
    tags: parTags
  }
}

resource webApp 'Microsoft.Web/sites@2023-01-01' = {
  name: varWebAppName
  location: parLocation
  kind: 'app,linux'
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
      linuxFxVersion: 'DOTNETCORE|8.0'
      netFrameworkVersion: '8.0'
      minTlsVersion: '1.2'

      healthCheckPath: '/api/health'

      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
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
          name: 'apim_base_url'
          value: apiManagement.properties.gatewayUrl
        }
        {
          name: 'apim_subscription_key'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${apiManagementSubscription.outputs.outSubscriptionName}-api-key-primary)'
        }
        {
          name: 'geolocation_api_application_audience'
          value: 'api://geolocation-api-${parEnvironment}-${parInstance}'
        }
        {
          name: 'APPINSIGHTS_PROFILERFEATURE_VERSION'
          value: '1.0.0'
        }
        {
          name: 'DiagnosticServices_EXTENSION_VERSION'
          value: '~3'
        }
      ]
    }
  }
}

module webTest 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/webtest:latest' = {
  name: '${deployment().name}-webtest'

  params: {
    parWebAppName: webApp.name
    parLocation: parLocation
    parTestUrl: 'https://${webApp.properties.defaultHostName}/api/health'
    parAppInsightsRef: parAppInsightsRef
    parTags: parTags
  }
}

module publicWebAppKeyVaultRoleAssignment 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvaultroleassignment:latest' = {
  name: '${deployment().name}-kvwebapprole'

  params: {
    parKeyVaultName: keyVault.name
    parRoleDefinitionId: keyVaultSecretUserRoleDefinition.id
    parPrincipalId: webApp.identity.principalId
  }
}

module webAppDns 'dnsWebApp.bicep' = {
  name: '${deployment().name}-dns'
  scope: resourceGroup(parDns.SubscriptionId, parDns.ResourceGroupName)

  params: {
    parDns: parDns
    parWebAppHostname: webApp.properties.defaultHostName
    parDomainAuthCode: webApp.properties.customDomainVerificationId
    parTags: parTags
  }
}

resource customDomain 'Microsoft.Web/sites/hostNameBindings@2023-01-01' = {
  name: '${parDns.Subdomain}.${parDns.Domain}'
  parent: webApp

  properties: {
    siteName: webApp.name
  }

  dependsOn: [
    webAppDns
  ]
}

// Outputs
output outWebAppIdentityPrincipalId string = webApp.identity.principalId
output outWebAppName string = webApp.name
