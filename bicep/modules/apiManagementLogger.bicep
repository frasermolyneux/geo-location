targetScope = 'resourceGroup'

// Parameters
param parApiManagementName string
param parAppInsightsInstrumentationKeySecretName string
param parKeyVaultUri string
param parAppInsightsName string
param parAppInsightsId string

// Existing Resources
resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: parApiManagementName
}

// Module Resources
resource appInsightsInstrumentationKeyNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-08-01' = {
  name: parAppInsightsInstrumentationKeySecretName
  parent: apiManagement

  properties: {
    displayName: parAppInsightsInstrumentationKeySecretName
    keyVault: {
      secretIdentifier: '${parKeyVaultUri}secrets/${parAppInsightsInstrumentationKeySecretName}'
    }
    secret: true
  }
}

resource apiManagementLogger 'Microsoft.ApiManagement/service/loggers@2021-08-01' = {
  name: parAppInsightsName
  parent: apiManagement

  properties: {
    credentials: {
      instrumentationKey: '{{${parAppInsightsInstrumentationKeySecretName}}}'
    }
    loggerType: 'applicationInsights'
    resourceId: parAppInsightsId
  }
}
