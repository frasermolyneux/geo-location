targetScope = 'resourceGroup'

// Parameters
@description('The app service plan name')
param appServicePlanName string

@description('The location to deploy the resources')
param location string = resourceGroup().location

@description('The tags to apply to the resources.')
param tags object = resourceGroup().tags

// Module Resources
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location

  sku: {
    name: 'B1'
    tier: 'Basic'
  }

  kind: 'linux'

  properties: {
    reserved: true
  }

  tags: tags
}

// Outputs
output outAppServicePlanRef object = {
  Name: appServicePlan.name
  ResourceGroupName: resourceGroup().name
  SubscriptionId: subscription().subscriptionId
}
