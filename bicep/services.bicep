targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parParentDnsName string
param parStrategicServicesSubscriptionId string
param parApimResourceGroupName string
param parApiManagementName string
param parWebAppsResourceGroupName string
param parAppServicePlanName string
param parTags object

// Variables
var varKeyVaultName = 'kv-geoloc-${parEnvironment}-${parLocation}'
var varAppInsightsName = 'ai-geolocation-${parEnvironment}-${parLocation}'

module lookupWebApp 'services/lookupWebApi.bicep' = {
  name: 'lookupWebApp'
  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppInsightsName: varAppInsightsName
    parParentDnsName: parParentDnsName
    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parApimResourceGroupName: parApimResourceGroupName
    parApiManagementName: parApiManagementName
    parWebAppsResourceGroupName: parWebAppsResourceGroupName
    parAppServicePlanName: parAppServicePlanName
    parTags: parTags
  }
}

module publicWebApp 'services/publicWebApp.bicep' = {
  name: 'publicWebApp'
  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppInsightsName: varAppInsightsName
    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parApimResourceGroupName: parApimResourceGroupName
    parApiManagementName: parApiManagementName
    parWebAppsResourceGroupName: parWebAppsResourceGroupName
    parAppServicePlanName: parAppServicePlanName
    parTags: parTags
  }
}
