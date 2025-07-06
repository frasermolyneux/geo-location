targetScope = 'resourceGroup'

// Parameters
@description('The environment for the resources')
param environment string

@description('The location to deploy the resources')
param location string = resourceGroup().location

@description('The instance name (e.g. 01, 02, 03)')
param instance string

@description('A reference to the key vault resource')
param keyVaultRef object

@description('A reference to the app insights resource')
param appInsightsRef object

@description('A reference to the app service plan resource')
param appServicePlanRef object

@description('A reference to the api management resource')
param apiManagementRef object

@description('The dns configuration object')
param dns object

@description('The tags to apply to the resources.')
param tags object = resourceGroup().tags

// Variables
var environmentUniqueId = uniqueString('app-geolocation-web', environment, instance)
var webAppName = 'app-geolocation-web-${environment}-${location}-${instance}-${environmentUniqueId}'

// Existing Resources
@description('https://learn.microsoft.com/en-gb/azure/role-based-access-control/built-in-roles#key-vault-secrets-user')
resource keyVaultSecretUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '4633458b-17de-408a-b874-0445c86b69e6'
}

resource keyVault 'Microsoft.KeyVault/vaults@2021-06-01-preview' existing = {
  name: keyVaultRef.Name
  scope: resourceGroup(keyVaultRef.SubscriptionId, keyVaultRef.ResourceGroupName)
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightsRef.Name
  scope: resourceGroup(appInsightsRef.SubscriptionId, appInsightsRef.ResourceGroupName)
}

resource appServicePlan 'Microsoft.Web/serverfarms@2020-10-01' existing = {
  name: appServicePlanRef.Name
  scope: resourceGroup(appServicePlanRef.SubscriptionId, appServicePlanRef.ResourceGroupName)
}

resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: apiManagementRef.Name
}

// Module Resources
module apiManagementSubscription 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/apimanagementsubscription:latest' = {
  name: '${deployment().name}-apimsubscription'

  params: {
    apiManagementName: apiManagement.name
    workloadName: webAppName
    scope: '/products/geolocation-api'
    keyVaultName: keyVault.name
    tags: tags
  }
}

resource webApp 'Microsoft.Web/sites@2023-01-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  tags: tags

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    serverFarmId: appServicePlan.id

    httpsOnly: true

    siteConfig: {
      ftpsState: 'Disabled'

      alwaysOn: true
      linuxFxVersion: 'DOTNETCORE|9.0'
      netFrameworkVersion: '9.0'
      minTlsVersion: '1.2'

      healthCheckPath: '/api/health'

      appSettings: [
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
          name: 'GeoLocationApi__BaseUrl'
          value: '${apiManagement.properties.gatewayUrl}/geolocation'
        }
        {
          name: 'GeoLocationApi__ApiKey'
          value: '@Microsoft.KeyVault(SecretUri=${apiManagementSubscription.outputs.primaryKeySecretRef.secretUri})'
        }
        {
          name: 'GeoLocationApi__ApplicationAudience'
          value: 'api://geolocation-api-${environment}-${instance}'
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

//module webTest 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/webtest:latest' = if (environment == 'prd') {
//  name: '${deployment().name}-webtest'
//
//  params: {
//    workloadName: webApp.name
//    testUrl: 'https://${webApp.properties.defaultHostName}/api/health'
//    appInsightsRef: appInsightsRef
//    location: location
//    tags: tags
//  }
//}

module publicWebAppKeyVaultRoleAssignment 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvaultroleassignment:latest' = {
  name: '${deployment().name}-kvwebapprole'

  params: {
    keyVaultName: keyVault.name
    principalId: webApp.identity.principalId
    roleDefinitionId: keyVaultSecretUserRoleDefinition.id
  }
}

module webAppDns 'dnsWebApp.bicep' = {
  name: '${deployment().name}-dns'
  scope: resourceGroup(dns.SubscriptionId, dns.ResourceGroupName)

  params: {
    dns: dns
    webAppHostname: webApp.properties.defaultHostName
    domainAuthCode: webApp.properties.customDomainVerificationId
    tags: tags
  }
}

resource customDomain 'Microsoft.Web/sites/hostNameBindings@2023-01-01' = {
  name: '${dns.Subdomain}.${dns.Domain}'
  parent: webApp

  properties: {
    siteName: webApp.name
  }

  dependsOn: [
    webAppDns
  ]
}

// Outputs
output webAppIdentityPrincipalId string = webApp.identity.principalId
output webAppName string = webApp.name
