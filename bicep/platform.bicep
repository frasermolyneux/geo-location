targetScope = 'subscription'

@description('The environment name (e.g. dev, test, prod)')
param parEnvironment string

@description('The location of the resource group and resources')
param parLocation string

@description('The instance name (e.g. 01, 02, 03, etc.)')
param parInstance string

@description('The tags to apply to the resources')
param parTags object

// Variables
var varResourceGroupName = 'rg-geolocation-${parEnvironment}-${parLocation}-${parInstance}'

// Module Resources
resource defaultResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: varResourceGroupName
  location: parLocation
  tags: parTags

  properties: {}
}
