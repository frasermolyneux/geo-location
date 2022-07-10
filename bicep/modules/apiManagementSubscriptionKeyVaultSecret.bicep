targetScope = 'resourceGroup'

// Parameters
param parKeyVaultName string
param parApiManagementSubscriptionName string
param parApiManagementSubscriptionId string
param parApiManagementResourceGroupName string
param parApiManagementName string
param parTags object

// Existing In-Scope Resources
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: parKeyVaultName
}

// Existing Out-Of-Scope Resources
resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: parApiManagementName
  scope: resourceGroup(parApiManagementSubscriptionId, parApiManagementResourceGroupName)
}

resource apiManagementSubscription 'Microsoft.ApiManagement/service/subscriptions@2021-08-01' existing = {
  name: parApiManagementSubscriptionName
  parent: apiManagement
}

// Module Resources
resource keyVaultSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: '${apiManagement.name}-${apiManagementSubscription.name}-apikey'
  parent: keyVault
  tags: parTags

  properties: {
    contentType: 'text/plain'
    value: apiManagementSubscription.properties.primaryKey
  }
}
