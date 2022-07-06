targetScope = 'resourceGroup'

// Parameters
param parAppInsightsName string
param parKeyVaultName string
param parLocation string

// Existing Resources
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: parKeyVaultName
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' existing = {
  name: 'log-platform-prd-uksouth'
  scope: resourceGroup('7760848c-794d-4a19-8cb2-52f71a21ac2b', 'rg-platform-logging-prd-uksouth')
}

// Module Resources
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: parAppInsightsName
  location: parLocation
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

resource appInsightsConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: '${appInsights.name}-connectionstring'
  parent: keyVault
  properties: {
    contentType: 'text/plain'
    value: appInsights.properties.ConnectionString
  }
}

resource appInsightsInstrumentationKeySecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: '${appInsights.name}-instrumentationkey'
  parent: keyVault
  properties: {
    contentType: 'text/plain'
    value: appInsights.properties.InstrumentationKey
  }
}

output outAppInsightsId string = appInsights.id
output outAppInsightsName string = appInsights.name
output outAppInsightsConnectionString string = appInsights.properties.ConnectionString
