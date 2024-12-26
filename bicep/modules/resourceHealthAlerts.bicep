targetScope = 'resourceGroup'

// Parameters
@description('The environment for the resources')
param environment string

@description('A reference to the action groups resource group')
param actionGroupResourceGroupRef object

@description('The tags to apply to the resources.')
param tags object = resourceGroup().tags

// Existing Resources
resource criticalActionGroup 'microsoft.insights/actiongroups@2024-10-01-preview' existing = {
  name: 'p0-critical-alerts-${environment}'
  scope: resourceGroup(actionGroupResourceGroupRef.SubscriptionId, actionGroupResourceGroupRef.ResourceGroupName)
}

resource highActionGroup 'microsoft.insights/actiongroups@2024-10-01-preview' existing = {
  name: 'p1-high-alerts-${environment}'
  scope: resourceGroup(actionGroupResourceGroupRef.SubscriptionId, actionGroupResourceGroupRef.ResourceGroupName)
}

resource moderateActionGroup 'microsoft.insights/actiongroups@2024-10-01-preview' existing = {
  name: 'p2-moderate-alerts-${environment}'
  scope: resourceGroup(actionGroupResourceGroupRef.SubscriptionId, actionGroupResourceGroupRef.ResourceGroupName)
}

resource lowActionGroup 'microsoft.insights/actiongroups@2024-10-01-preview' existing = {
  name: 'p3-low-alerts-${environment}'
  scope: resourceGroup(actionGroupResourceGroupRef.SubscriptionId, actionGroupResourceGroupRef.ResourceGroupName)
}

resource informationalActionGroup 'microsoft.insights/actiongroups@2024-10-01-preview' existing = {
  name: 'p4-informational-alerts-${environment}'
  scope: resourceGroup(actionGroupResourceGroupRef.SubscriptionId, actionGroupResourceGroupRef.ResourceGroupName)
}

// ModuleResources
resource resourceHealthAlerts 'Microsoft.Insights/activityLogAlerts@2020-10-01' = {
  name: 'geolocation-${environment} - ${resourceGroup().name} - resource health'
  location: 'global'
  properties: {
    scopes: [
      '/subscriptions/${subscription().subscriptionId}/resourceGroups/${resourceGroup().name}'
    ]
    condition: {
      allOf: [
        {
          field: 'category'
          equals: 'ResourceHealth'
        }
        {
          anyOf: [
            {
              field: 'properties.previousHealthStatus'
              equals: 'Available'
            }
          ]
        }
      ]
    }
    actions: {
      actionGroups: [
        {
          actionGroupId: environment == 'prd' ? criticalActionGroup.id : informationalActionGroup.id
          webhookProperties: {}
        }
      ]
    }
    enabled: true
    description: 'Resource health alert for ${resourceGroup().name} resource group'
  }
  tags: tags
}
