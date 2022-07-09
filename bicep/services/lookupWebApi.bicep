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
var varFrontDoorName = 'fd-webapi-geolocation-lookup-${parEnvironment}'
var varFrontDoorDns = 'webapi-geolocation-lookup-${parEnvironment}'

// Existing Resources
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: parKeyVaultName
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: parAppInsightsName
}

resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: parApiManagementName
}

// Module Resources
module scopedLookupWebApi 'modules/scopedLookupWebApi.bicep' = {
  name: 'scopedLookupWebApi'
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

resource webAppKeyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2021-11-01-preview' = {
  name: 'add'
  parent: keyVault

  properties: {
    accessPolicies: [
      {
        objectId: scopedLookupWebApi.outputs.outWebAppIdentityPrincipalId
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

module lookupWebApiFrontDoor 'modules/frontDoor.bicep' = {
  name: 'lookupWebApiFrontDoor'

  params: {
    parFrontDoorName: varFrontDoorName
    parFrontDoorDns: varFrontDoorDns
    parParentDnsName: parParentDnsName
    parConnectivitySubscriptionId: parConnectivitySubscriptionId
    parDnsResourceGroupName: parDnsResourceGroupName
    parOriginHostName: scopedLookupWebApi.outputs.outWebAppDefaultHostName
    parTags: parTags
  }
}

resource apiBackend 'Microsoft.ApiManagement/service/backends@2021-08-01' = {
  name: varFrontDoorDns
  parent: apiManagement

  properties: {
    title: varFrontDoorDns
    description: varFrontDoorDns
    url: 'https://${varFrontDoorDns}.${parParentDnsName}/'
    protocol: 'http'
    properties: {}

    tls: {
      validateCertificateChain: true
      validateCertificateName: true
    }
  }
}

resource apiActiveBackendNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-08-01' = {
  name: 'lookup-api-active-backend'
  parent: apiManagement

  properties: {
    displayName: 'lookup-api-active-backend'
    value: apiBackend.name
    secret: false
  }
}

resource apiAudienceNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-08-01' = {
  name: 'lookup-api-audience'
  parent: apiManagement

  properties: {
    displayName: 'lookup-api-audience'
    keyVault: {
      secretIdentifier: '${keyVault.properties.vaultUri}secrets/geolocation-lookup-api-${parEnvironment}-clientid'
    }
    secret: true
  }
}

resource api 'Microsoft.ApiManagement/service/apis@2021-08-01' = {
  name: 'api'
  parent: apiManagement

  properties: {
    apiRevision: '1.0'
    apiType: 'http'
    type: 'http'

    displayName: 'GeoLocation Lookup API'
    path: ''

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

resource apiPolicy 'Microsoft.ApiManagement/service/apis/policies@2021-08-01' = {
  name: 'policy'
  parent: api
  properties: {
    format: 'xml'
    value: '''
<policies>
  <inbound>
      <base/>
      <set-backend-service backend-id="{{lookup-api-active-backend}}" />
      <cache-lookup vary-by-developer="false" vary-by-developer-groups="false" downstream-caching-type="none" />
      <validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="JWT validation was unsuccessful" require-expiration-time="true" require-scheme="Bearer" require-signed-tokens="true">
          <openid-config url="{{tenant-login-url}}{{tenant-id}}/v2.0/.well-known/openid-configuration" />
          <audiences>
              <audience>{{lookup-api-audience}}</audience>
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
