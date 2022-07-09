targetScope = 'resourceGroup'

// Parameters
param parApiManagementName string
param parWorkloadName string

// Existing Resources
resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: parApiManagementName
}

// Module Resources
resource apiManagementSubscription 'Microsoft.ApiManagement/service/subscriptions@2021-08-01' = {
  name: '${apiManagement.name}-${parWorkloadName}-subscription'
  parent: apiManagement

  properties: {
    allowTracing: false
    displayName: parWorkloadName
    scope: '/apis'
  }
}

// Outputs
output outApiManagementSubcriptionKey string = apiManagementSubscription.properties.primaryKey
