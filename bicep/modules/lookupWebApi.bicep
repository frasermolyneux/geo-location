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

@description('The tags to apply to the resources.')
param tags object = resourceGroup().tags

// Variables
var environmentUniqueId = uniqueString('app-geolocation-api', environment, instance)
var webAppName = 'app-geolocation-api-${environment}-${location}-${instance}-${environmentUniqueId}'

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
module appDataStorage 'appDataStorage.bicep' = {
  name: '${deployment().name}-appdata'

  params: {
    keyVaultName: keyVault.name
    location: location
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
          name: 'AzureAd__TenantId'
          value: tenant().tenantId
        }
        {
          name: 'AzureAd__Instance'
          value: az.environment().authentication.loginEndpoint
        }
        {
          name: 'AzureAd__ClientId'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=geolocation-api-${environment}-clientid)'
        }
        {
          name: 'AzureAd__ClientSecret'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=geolocation-api-${environment}-clientsecret)'
        }
        {
          name: 'AzureAd__Audience'
          value: 'api://geolocation-api-${environment}-${instance}'
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
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${appDataStorage.outputs.outStorageAccountName}-connectionstring)'
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

module lookupWebApiKeyVaultRoleAssignment 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvaultroleassignment:latest' = {
  name: '${deployment().name}-kvwebapirole'

  params: {
    keyVaultName: keyVault.name
    principalId: webApp.identity.principalId
    roleDefinitionId: keyVaultSecretUserRoleDefinition.id
  }
}

resource apiBackend 'Microsoft.ApiManagement/service/backends@2021-08-01' = {
  name: '${deployment().name}-apiBackend'
  parent: apiManagement

  properties: {
    title: webApp.properties.defaultHostName
    description: webApp.properties.defaultHostName
    url: 'https://${webApp.properties.defaultHostName}/'
    protocol: 'http'
    properties: {}

    tls: {
      validateCertificateChain: true
      validateCertificateName: true
    }
  }
}

resource apiActiveBackendNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-08-01' = {
  name: 'geolocation-active-backend'
  parent: apiManagement

  properties: {
    displayName: 'geolocation-active-backend'
    value: apiBackend.name
    secret: false
  }
}

resource apiAudienceNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-08-01' = {
  name: 'geolocation-api-audience'
  parent: apiManagement

  properties: {
    displayName: 'geolocation-api-audience'
    value: 'api://geolocation-api-${environment}-${instance}'
    secret: false
  }
}

resource api 'Microsoft.ApiManagement/service/apis@2021-08-01' = {
  name: 'geolocation-api'
  parent: apiManagement

  properties: {
    apiRevision: '1.0'
    apiType: 'http'
    type: 'http'

    displayName: 'GeoLocation Lookup API'
    path: 'geolocation'

    protocols: [
      'https'
    ]

    subscriptionRequired: true
    subscriptionKeyParameterNames: {
      header: 'Ocp-Apim-Subscription-Key'
    }

    format: 'openapi+json'
    value: loadTextContent('./../../.azure-pipelines/api-definitions/lookup-api.openapi+json.json')
  }
}

resource tenantIdNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-08-01' = {
  name: 'tenant-id'
  parent: apiManagement

  properties: {
    displayName: 'tenant-id'
    value: tenant().tenantId
    secret: false
  }
}

resource tenantLoginUrlNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-08-01' = {
  name: 'tenant-login-url'
  parent: apiManagement

  properties: {
    displayName: 'tenant-login-url'
    value: az.environment().authentication.loginEndpoint
    secret: false
  }
}

resource apiPolicy 'Microsoft.ApiManagement/service/apis/policies@2021-08-01' = {
  name: 'policy'
  parent: api
  properties: {
    format: 'xml'
    value: '''
<policies>
  <inbound>
      <base/>
      <set-backend-service backend-id="{{geolocation-active-backend}}" />
      <cache-lookup vary-by-developer="false" vary-by-developer-groups="false" downstream-caching-type="none" />
      <validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="JWT validation was unsuccessful" require-expiration-time="true" require-scheme="Bearer" require-signed-tokens="true">
          <openid-config url="{{tenant-login-url}}{{tenant-id}}/v2.0/.well-known/openid-configuration" />
          <audiences>
              <audience>{{geolocation-api-audience}}</audience>
          </audiences>
          <issuers>
              <issuer>https://sts.windows.net/{{tenant-id}}/</issuer>
          </issuers>
          <required-claims>
              <claim name="roles" match="any">
                <value>LookupApiUser</value>
              </claim>
          </required-claims>
      </validate-jwt>
  </inbound>
  <backend>
      <forward-request />
  </backend>
  <outbound>
      <base/>
      <cache-store duration="3600" />
  </outbound>
  <on-error />
</policies>'''
  }

  dependsOn: [
    apiActiveBackendNamedValue
    apiAudienceNamedValue
    tenantIdNamedValue
    tenantLoginUrlNamedValue
  ]
}

resource apiDiagnostics 'Microsoft.ApiManagement/service/apis/diagnostics@2021-08-01' = {
  name: 'applicationinsights'
  parent: api

  properties: {
    alwaysLog: 'allErrors'

    httpCorrelationProtocol: 'W3C'
    logClientIp: true
    loggerId: resourceId('Microsoft.ApiManagement/service/loggers', apiManagement.name, appInsights.name)
    operationNameFormat: 'Name'

    sampling: {
      percentage: 100
      samplingType: 'fixed'
    }

    verbosity: 'information'
  }
}

// Outputs
output webAppIdentityPrincipalId string = webApp.identity.principalId
output webAppName string = webApp.name
