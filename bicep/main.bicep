targetScope = 'subscription'

@description('The environment for the resources')
param environment string

@description('The location to deploy the resources')
param location string

@description('The instance name (e.g. 01, 02, 03, etc.)')
param instance string

@description('A reference to the log analytics workspace resource')
param logAnalyticsWorkspaceRef object

@description('The dns configuration object')
param dns object

@description('The tags to apply to the resources')
param tags object

@description('The key vault create mode (recover, default)')
param keyVaultCreateMode string = 'recover'

// Variables
var environmentUniqueId = uniqueString('geolocation', environment, instance)
var resourceGroupName = 'rg-geolocation-${environment}-${location}-${instance}'
var keyVaultName = 'kv-${environmentUniqueId}-${location}'
var varAppInsightsName = 'ai-geolocation-${environment}-${location}-${instance}'
var varAppServicePlanName = 'plan-geolocation-${environment}-${location}-${instance}'
var varApiManagementName = 'apim-geolocation-${environment}-${location}-${environmentUniqueId}'

// Module Resources
resource defaultResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
  tags: tags

  properties: {}
}

module keyVault 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvault:latest' = {
  name: '${deployment().name}-keyvault'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    keyVaultName: keyVaultName
    keyVaultCreateMode: keyVaultCreateMode
    location: location
    tags: tags
  }
}

module appInsights 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/appinsights:latest' = {
  name: '${deployment().name}-appInsights'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    appInsightsName: varAppInsightsName
    logAnalyticsWorkspaceRef: logAnalyticsWorkspaceRef
    location: location
    tags: tags
  }
}

module appServicePlan 'modules/appServicePlan.bicep' = {
  name: '${deployment().name}-appServicePlan'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    appServicePlanName: varAppServicePlanName
    location: location
    tags: tags
  }
}

module apiManagement 'modules/apiManagement.bicep' = {
  name: '${deployment().name}-apiManagement'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    apiManagementName: varApiManagementName
    location: location
    tags: tags
  }
}

module apiManagementLogger 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/apimanagementlogger:latest' = {
  name: '${deployment().name}-apimlogger'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    apiManagementName: apiManagement.outputs.outApiManagementRef.Name
    appInsightsName: varAppInsightsName
  }
}

module lookupWebApi 'modules/lookupWebApi.bicep' = {
  name: '${deployment().name}-lookupWebApi'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    environment: environment
    location: location
    instance: instance
    keyVaultRef: {
      Name: keyVaultName
      ResourceGroupName: defaultResourceGroup.name
      SubscriptionId: subscription().subscriptionId
    }
    appInsightsRef: {
      Name: appInsights.outputs.appInsightsRef.Name
      ResourceGroupName: defaultResourceGroup.name
      SubscriptionId: subscription().subscriptionId
    }
    appServicePlanRef: appServicePlan.outputs.outAppServicePlanRef
    apiManagementRef: apiManagement.outputs.outApiManagementRef
    tags: tags
  }
}

module publicWebApp 'modules/publicWebApp.bicep' = {
  name: '${deployment().name}-publicWebApp'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    environment: environment
    location: location
    instance: instance
    keyVaultRef: {
      Name: keyVaultName
      ResourceGroupName: defaultResourceGroup.name
      SubscriptionId: subscription().subscriptionId
    }
    appInsightsRef: {
      Name: appInsights.outputs.appInsightsRef.Name
      ResourceGroupName: defaultResourceGroup.name
      SubscriptionId: subscription().subscriptionId
    }
    appServicePlanRef: appServicePlan.outputs.outAppServicePlanRef
    apiManagementRef: apiManagement.outputs.outApiManagementRef
    dns: dns
    tags: tags
  }
}

// Outputs
output webAppIdentityPrincipalId string = publicWebApp.outputs.webAppIdentityPrincipalId
output outResourceGroupName string = defaultResourceGroup.name
output webAppName string = publicWebApp.outputs.webAppName
output outWebApiName string = lookupWebApi.outputs.webAppName
output outKeyVaultName string = keyVault.outputs.keyVaultRef.name
