targetScope = 'resourceGroup'

// Parameters
param parDeploymentPrefix string
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parTags object

// Existing Out-Of-Scope Resources
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: parKeyVaultName
}

// Module Resources
resource storageAccount 'Microsoft.Storage/storageAccounts@2019-06-01' = {
  name: 'salkupappdata${parEnvironment}'
  location: parLocation
  kind: 'StorageV2'
  tags: parTags

  sku: {
    name: 'Standard_LRS'
  }
}

resource tableServices 'Microsoft.Storage/storageAccounts/tableServices@2021-09-01' = {
  name: 'default'
  parent: storageAccount

  properties: {}
}

resource geoLocationsTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2021-09-01' = {
  name: 'geolocations'
  parent: tableServices

  properties: {}
}

module keyVaultSecret './../../modules/keyVaultSecret.bicep' = {
  name: '${parDeploymentPrefix}-${storageAccount.name}-keyVaultSecret'

  params: {
    parKeyVaultName: parKeyVaultName
    parSecretName: '${storageAccount.name}-connectionstring'
    parSecretValue: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'
    parTags: parTags
  }
}

// Outputs
output outStorageAccountName string = storageAccount.name
