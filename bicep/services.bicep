targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parManagementSubscriptionId string
param parDnsResourceGroupName string
param parParentDnsName string
param parTags object

// Variables
var varKeyVaultName = 'kv-geoloc-${parEnvironment}-${parLocation}'
var varAppInsightsName = 'ai-geolocation-${parEnvironment}-${parLocation}'
var varApimName = 'apim-geolocation-${parEnvironment}-${parLocation}'
var varAppServicePlanName = 'plan-geolocation-${parEnvironment}-${parLocation}'

module lookupWebApp 'services/lookupWebApi.bicep' = {
  name: 'lookupWebApp'
  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppServicePlanName: varAppServicePlanName
    parAppInsightsName: varAppInsightsName
    parApiManagementName: varApimName
    parManagementSubscriptionId: parManagementSubscriptionId
    parDnsResourceGroupName: parDnsResourceGroupName
    parParentDnsName: parParentDnsName
    parTags: parTags
  }
}

module publicWebApp 'services/publicWebApp.bicep' = {
  name: 'publicWebApp'
  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppServicePlanName: varAppServicePlanName
    parAppInsightsName: varAppInsightsName
    parApiManagementName: varApimName
    parManagementSubscriptionId: parManagementSubscriptionId
    parDnsResourceGroupName: parDnsResourceGroupName
    parParentDnsName: parParentDnsName
    parTags: parTags
  }
}
