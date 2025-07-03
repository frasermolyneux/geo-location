targetScope = 'resourceGroup'

// Parameters
@description('The location to deploy the resources')
param location string = resourceGroup().location

@description('The external api consumer object')
param externalApiConsumer object

@description('A reference to the api management resource')
param apiManagementRef object

@description('The tags to apply to the resources.')
param tags object = resourceGroup().tags

// Variables
var environmentUniqueId = uniqueString('geolocation', externalApiConsumer.Workload, externalApiConsumer.PrincipalId)
var keyVaultName = 'kv-${environmentUniqueId}-${location}'

// Module Resources
resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: apiManagementRef.Name
}

module keyVault 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvault:latest' = {
  name: '${externalApiConsumer.Workload}-kv'

  params: {
    keyVaultName: keyVaultName
    keyVaultCreateMode: 'default'
    location: location
    tags: union(tags, {
      consumerWorkload: externalApiConsumer.Workload
      consumerPricipalId: externalApiConsumer.PrincipalId
    })
  }
}

@description('https://learn.microsoft.com/en-gb/azure/role-based-access-control/built-in-roles#key-vault-secrets-user')
resource keyVaultSecretUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '4633458b-17de-408a-b874-0445c86b69e6'
}

module keyVaultRoleAssignment 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvaultroleassignment:latest' = {
  name: '${externalApiConsumer.Workload}-kvrole'

  params: {
    keyVaultName: keyVault.outputs.keyVaultRef.name
    principalId: externalApiConsumer.PrincipalId
    roleDefinitionId: keyVaultSecretUserRoleDefinition.id
  }
}

module apiManagementSubscription 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/apimanagementsubscription:latest' = {
  name: '${externalApiConsumer.Workload}-apimsubscription'

  params: {
    apiManagementName: apiManagement.name
    workloadName: externalApiConsumer.Workload
    scope: '/products/geolocation-api'
    keyVaultName: keyVault.outputs.keyVaultRef.name
    tags: tags
  }
}
