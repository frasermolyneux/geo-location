targetScope = 'subscription'

// Parameters
param parLocation string
param parEnvironment string

// Variables
var varResourceGroupName = 'rg-geolocation-${parEnvironment}-${parLocation}'

resource portalResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: varResourceGroupName
  location: parLocation
  properties: {}
}
