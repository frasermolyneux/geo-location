targetScope = 'resourceGroup'

// Parameters
@description('The app service plan name')
param parAppServicePlanName string

@description('The location of the resource group.')
param parLocation string = resourceGroup().location

@description('The tags to apply to the resources.')
param parTags object = resourceGroup().tags

// Module Resources
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: parAppServicePlanName
  location: parLocation

  sku: {
    name: 'B1'
    tier: 'Basic'
  }

  kind: 'linux'
  tags: parTags
}

// Outputs
output outAppServicePlanRef object = {
  Name: appServicePlan.name
  ResourceGroupName: resourceGroup().name
  SubscriptionId: subscription().subscriptionId
}
