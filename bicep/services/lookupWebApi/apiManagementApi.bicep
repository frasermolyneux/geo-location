targetScope = 'resourceGroup'

// Parameters
@description('The environment name (e.g. dev, test, prod).')
param parEnvironment string

@description('The instance name (e.g. 01, 02, 03).')
param parInstance string

@description('The name of the API Management instance.')
param parApiManagementName string

@description('The name of the Front Door DNS.')
param parFrontDoorDns string

@description('The name of the parent DNS.')
param parParentDnsName string

@description('The subscription ID of the workload resource group.')
param parWorkloadSubscriptionId string

@description('The name of the workload resource group.')
param parWorkloadResourceGroupName string

@description('The name of the Application Insights instance.')
param parAppInsightsName string

// Existing In-Scope Resources
@description('Reference to the existing API Management instance.')
resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: parApiManagementName
}

// Existing Out-Of-Scope Resources
@description('Reference to the existing application insights.')
resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: parAppInsightsName
  scope: resourceGroup(parWorkloadSubscriptionId, parWorkloadResourceGroupName)
}

// Module Resources
@description('API Management Backend.')
resource apiBackend 'Microsoft.ApiManagement/service/backends@2021-08-01' = {
  name: parFrontDoorDns
  parent: apiManagement

  properties: {
    title: parFrontDoorDns
    description: parFrontDoorDns
    url: 'https://${parFrontDoorDns}.${parParentDnsName}/'
    protocol: 'http'
    properties: {}

    tls: {
      validateCertificateChain: true
      validateCertificateName: true
    }
  }
}

@description('API Management Named Value (Active Backend).')
resource apiActiveBackendNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-08-01' = {
  name: 'geolocation-active-backend'
  parent: apiManagement

  properties: {
    displayName: 'geolocation-active-backend'
    value: apiBackend.name
    secret: false
  }
}

@description('API Management Named Value (API Audience).')
resource apiAudienceNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-08-01' = {
  name: 'geolocation-api-audience'
  parent: apiManagement

  properties: {
    displayName: 'geolocation-api-audience'
    value: 'api://geolocation-api-${parEnvironment}-${parInstance}'
    secret: false
  }
}

@description('API Management API.')
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
    value: loadTextContent('./../../../.azure-pipelines/api-definitions/lookup-api.openapi+json.json')
  }
}

@description('API Management API Policy.')
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
  ]
}

@description('API Management API Diagnostics.')
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
