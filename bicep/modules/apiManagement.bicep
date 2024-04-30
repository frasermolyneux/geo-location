targetScope = 'resourceGroup'

// Parameters
@description('The API Management name')
param parApiManagementName string

@description('The location of the resource group.')
param parLocation string = resourceGroup().location

@description('The tags to apply to the resources.')
param parTags object = resourceGroup().tags

// Module Resources
resource apiManagement 'Microsoft.ApiManagement/service@2023-05-01-preview' = {
  name: parApiManagementName
  location: parLocation

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

  tags: parTags
}

// Outputs
output outApiManagementRef object = {
  Name: apiManagement.name
  ResourceGroup: resourceGroup().name
  Subscription: subscription().subscriptionId
}
