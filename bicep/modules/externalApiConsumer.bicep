targetScope = 'resourceGroup'

// Parameters
@description('The location of the resource group.')
param parLocation string = resourceGroup().location

@description('The external api consumer object')
param parExternalApiConsumer object

@description('The api management Ref')
param parApiManagementRef object

@description('The tags to apply to the resources.')
param parTags object = resourceGroup().tags

// Variables
var varEnvironmentUniqueId = uniqueString(
  'geolocation',
  parExternalApiConsumer.Workload,
  parExternalApiConsumer.PrincipalId
)
var varKeyVaultName = 'kv-${varEnvironmentUniqueId}-${parLocation}'

// Module Resources
resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: parApiManagementRef.Name
}

module keyVault 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvault:latest' = {
  name: '${parExternalApiConsumer.Workload}-kv'

  params: {
    parKeyVaultName: varKeyVaultName
    parLocation: parLocation
    parEnabledForRbacAuthorization: true
    parTags: union(parTags, {
      consumerWorkload: parExternalApiConsumer.Workload
      consumerPricipalId: parExternalApiConsumer.PrincipalId
    })
    parKeyVaultCreateMode: 'default'
  }
}

@description('https://learn.microsoft.com/en-gb/azure/role-based-access-control/built-in-roles#key-vault-secrets-user')
resource keyVaultSecretUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '4633458b-17de-408a-b874-0445c86b69e6'
}

module keyVaultRoleAssignment 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvaultroleassignment:latest' = {
  name: '${parExternalApiConsumer.Workload}-kvrole'

  params: {
    parKeyVaultName: keyVault.outputs.outKeyVaultName
    parRoleDefinitionId: keyVaultSecretUserRoleDefinition.id
    parPrincipalId: parExternalApiConsumer.PrincipalId
  }
}

module apiManagementSubscription 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/apimanagementsubscription:latest' = {
  name: '${parExternalApiConsumer.Workload}-apimsubscription'

  params: {
    apiManagementName: apiManagement.name
    subscriptionName: parExternalApiConsumer.Workload
    apiScope: 'geolocation-api'
    keyVaultName: keyVault.outputs.outKeyVaultName
    tags: parTags
  }
}
