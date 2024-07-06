targetScope = 'resourceGroup'

// Parameters
@description('The api management resource name')
param apiManagementName string

@description('The location to deploy the resources')
param location string = resourceGroup().location

@description('The tags to apply to the resources.')
param tags object = resourceGroup().tags

// Module Resources
resource apiManagement 'Microsoft.ApiManagement/service@2023-05-01-preview' = {
  name: apiManagementName
  location: location

  sku: {
    capacity: 0
    name: 'Consumption'
  }

  properties: {
    publisherEmail: 'admin@molyneux.io'
    publisherName: 'Molyneux.IO'
  }

  identity: {
    type: 'SystemAssigned'
  }

  tags: tags
}

// Outputs
output outApiManagementRef object = {
  Name: apiManagement.name
  ResourceGroupName: resourceGroup().name
  SubscriptionId: subscription().subscriptionId
}
