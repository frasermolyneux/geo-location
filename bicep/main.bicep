targetScope = 'subscription'

@description('The environment name (e.g. dev, test, prod)')
param parEnvironment string

@description('The location of the resource group and resources')
param parLocation string

@description('The instance name (e.g. 01, 02, 03, etc.)')
param parInstance string

@description('The logging workspace details (subscriptionId, resourceGroupName, workspaceName)')
param parLogging object

@description('The dns configuration object')
param parDns object

@description('The tags to apply to the resources')
param parTags object

@description('The key vault create mode (recover, default)')
param parKeyVaultCreateMode string = 'recover'

// Variables
var varEnvironmentUniqueId = uniqueString('geolocation', parEnvironment, parInstance)
var varResourceGroupName = 'rg-geolocation-${parEnvironment}-${parLocation}-${parInstance}'
var varKeyVaultName = 'kv-${varEnvironmentUniqueId}-${parLocation}'
var varAppInsightsName = 'ai-geolocation-${parEnvironment}-${parLocation}-${parInstance}'
var varAppServicePlanName = 'plan-geolocation-${parEnvironment}-${parLocation}-${parInstance}'
var varApiManagementName = 'apim-geolocation-${parEnvironment}-${parLocation}-${varEnvironmentUniqueId}'

// Module Resources
resource defaultResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: varResourceGroupName
  location: parLocation
  tags: parTags

  properties: {}
}

module keyVault 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvault:latest' = {
  name: '${deployment().name}-keyvault'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    parKeyVaultName: varKeyVaultName
    parLocation: parLocation
    parKeyVaultCreateMode: parKeyVaultCreateMode
    parEnabledForRbacAuthorization: true
    parTags: parTags
  }
}

module appInsights 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/appinsights:latest' = {
  name: '${deployment().name}-appInsights'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    parAppInsightsName: varAppInsightsName
    parLocation: parLocation
    parLoggingSubscriptionId: parLogging.SubscriptionId
    parLoggingResourceGroupName: parLogging.WorkspaceResourceGroupName
    parLoggingWorkspaceName: parLogging.WorkspaceName
    parTags: parTags
  }
}

module appServicePlan 'modules/appServicePlan.bicep' = {
  name: '${deployment().name}-appServicePlan'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    parAppServicePlanName: varAppServicePlanName
    parLocation: parLocation
    parTags: parTags
  }
}

module apiManagement 'modules/apiManagement.bicep' = {
  name: '${deployment().name}-apiManagement'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    parApiManagementName: varApiManagementName
    parLocation: parLocation
    parTags: parTags
  }
}

module apiManagementLogger 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/apimanagementlogger:latest' = {
  name: '${deployment().name}-apimlogger'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    parApiManagementName: apiManagement.outputs.outApiManagementRef.Name

    parAppInsightsRef: {
      Name: varAppInsightsName
      SubscriptionId: subscription().subscriptionId
      ResourceGroupName: defaultResourceGroup.name
    }
  }
}

module lookupWebApi 'modules/lookupWebApi.bicep' = {
  name: '${deployment().name}-lookupWebApi'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    parEnvironment: parEnvironment
    parLocation: parLocation
    parInstance: parInstance
    parKeyVaultRef: {
      Name: varKeyVaultName
      ResourceGroupName: defaultResourceGroup.name
      SubscriptionId: subscription().subscriptionId
    }
    parAppInsightsRef: {
      Name: appInsights.outputs.outAppInsightsName
      ResourceGroupName: defaultResourceGroup.name
      SubscriptionId: subscription().subscriptionId
    }
    parAppServicePlanRef: appServicePlan.outputs.outAppServicePlanRef
    parApiManagementRef: apiManagement.outputs.outApiManagementRef
    parTags: parTags
  }
}

module publicWebApp 'modules/publicWebApp.bicep' = {
  name: '${deployment().name}-publicWebApp'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    parEnvironment: parEnvironment
    parLocation: parLocation
    parInstance: parInstance
    parKeyVaultRef: {
      Name: varKeyVaultName
      ResourceGroupName: defaultResourceGroup.name
      SubscriptionId: subscription().subscriptionId
    }
    parAppInsightsRef: {
      Name: appInsights.outputs.outAppInsightsName
      ResourceGroupName: defaultResourceGroup.name
      SubscriptionId: subscription().subscriptionId
    }
    parAppServicePlanRef: appServicePlan.outputs.outAppServicePlanRef
    parApiManagementRef: apiManagement.outputs.outApiManagementRef
    parDns: parDns
    parTags: parTags
  }
}

// Outputs
output outWebAppIdentityPrincipalId string = publicWebApp.outputs.outWebAppIdentityPrincipalId
output outResourceGroupName string = defaultResourceGroup.name
output outWebAppName string = publicWebApp.outputs.outWebAppName
output outWebApiName string = lookupWebApi.outputs.outWebAppName
output outKeyVaultName string = keyVault.outputs.outKeyVaultName
