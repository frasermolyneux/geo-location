targetScope = 'subscription'

// Parameters
@description('The environment for the resources')
param environment string

@description('The location to deploy the resources')
param location string

@description('The instance name (e.g. 01, 02, 03, etc.)')
param instance string

@description('The tags to apply to the resources')
param tags object

@description('The external api consumers')
param externalApiConsumers array = []

// Variables
var environmentUniqueId = uniqueString('geolocation', environment, instance)
var resourceGroupName = 'rg-geolocation-${environment}-${location}-${instance}'
var varApiManagementName = 'apim-geolocation-${environment}-${location}-${environmentUniqueId}'

// Module Resources
resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: varApiManagementName
  scope: resourceGroup(resourceGroupName)
}

// Reference the existing product to ensure it exists before creating consumers
resource existingProduct 'Microsoft.ApiManagement/service/products@2021-08-01' existing = {
  name: 'geolocation-api'
  parent: apiManagement
}

module externalApiConsumer 'modules/externalApiConsumer.bicep' = [
  for consumer in externalApiConsumers: {
    name: '${deployment().name}-${consumer.Workload}'
    scope: resourceGroup(resourceGroupName)

    params: {
      location: location
      externalApiConsumer: consumer
      apiManagementRef: {
        Name: apiManagement.name
        ResourceGroupName: resourceGroupName
        SubscriptionId: subscription().subscriptionId
      }
      tags: tags
    }

    dependsOn: [
      existingProduct
    ]
  }
]
