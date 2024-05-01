targetScope = 'subscription'

// Parameters
@description('The environment name (e.g. dev, test, prod)')
param parEnvironment string

@description('The location of the resource group and resources')
param parLocation string

@description('The instance name (e.g. 01, 02, 03, etc.)')
param parInstance string

@description('The tags to apply to the resources')
param parTags object

@description('The external api consumers')
param parExternalApiConsumers array = []

// Variables
var varEnvironmentUniqueId = uniqueString('geolocation', parEnvironment, parInstance)
var varResourceGroupName = 'rg-geolocation-${parEnvironment}-${parLocation}-${parInstance}'
var varApiManagementName = 'apim-geolocation-${parEnvironment}-${parLocation}-${varEnvironmentUniqueId}'

// Module Resources
resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: varApiManagementName
  scope: resourceGroup(varResourceGroupName)
}

module externalApiConsumer 'modules/externalApiConsumer.bicep' = [
  for consumer in parExternalApiConsumers: {
    name: '${deployment().name}-${consumer.Workload}'
    scope: resourceGroup(varResourceGroupName)

    params: {
      parLocation: parLocation
      parExternalApiConsumer: consumer
      parApiManagementRef: {
        Name: apiManagement.name
        ResourceGroupName: varResourceGroupName
        SubscriptionId: subscription().subscriptionId
      }
      parTags: parTags
    }
  }
]
