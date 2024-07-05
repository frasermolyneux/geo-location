targetScope = 'resourceGroup'

// Parameters
@description('The name of the Key Vault to store the secrets in.')
param parKeyVaultName string

@description('The location to deploy the resources to.')
param parLocation string = resourceGroup().location

@description('The tags to apply to all resources in this deployment.')
param parTags object = resourceGroup().tags

// Module Resources
@description('The storage account')
resource storageAccount 'Microsoft.Storage/storageAccounts@2019-06-01' = {
  name: 'saad${uniqueString(resourceGroup().name)}'
  location: parLocation
  kind: 'StorageV2'
  tags: parTags

  sku: {
    name: 'Standard_LRS'
  }
}

@description('The table services')
resource tableServices 'Microsoft.Storage/storageAccounts/tableServices@2021-09-01' = {
  name: 'default'
  parent: storageAccount

  properties: {}
}

@description('The geo locations table')
resource geoLocationsTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2021-09-01' = {
  name: 'geolocations'
  parent: tableServices

  properties: {}
}

@description('Key vault secret for storage connection string')
module keyVaultSecret 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvaultsecret:latest' = {
  name: '${storageAccount.name}-kvsecret'

  params: {
    keyVaultName: parKeyVaultName
    secretName: '${storageAccount.name}-connectionstring'
    secretValue: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
    tags: parTags
  }
}

// Outputs
output outStorageAccountName string = storageAccount.name
