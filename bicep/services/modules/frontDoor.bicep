targetScope = 'resourceGroup'

// Parameters
param parFrontDoorName string
param parFrontDoorDns string
param parParentDnsName string
param parManagementSubscriptionId string
param parDnsResourceGroupName string
param parOriginHostName string
param parTags object

// Existing Resources
resource parentDnsZone 'Microsoft.Network/dnsZones@2018-05-01' existing = {
  name: parParentDnsName
  scope: resourceGroup(parManagementSubscriptionId, parDnsResourceGroupName)
}

// Module Resources
resource frontDoor 'Microsoft.Cdn/profiles@2021-06-01' = {
  name: parFrontDoorName
  location: 'Global'
  tags: parTags

  sku: {
    name: 'Standard_AzureFrontDoor'
  }

  properties: {
    originResponseTimeoutSeconds: 60
  }
}

resource frontDoorEndpoint 'Microsoft.Cdn/profiles/afdendpoints@2021-06-01' = {
  parent: frontDoor
  name: 'default-endpoint'
  location: 'Global'
  tags: parTags

  properties: {
    enabledState: 'Enabled'
  }
}

resource frontDoorOriginGroup 'Microsoft.Cdn/profiles/origingroups@2021-06-01' = {
  parent: frontDoor
  name: 'default-origin-group'

  properties: {
    loadBalancingSettings: {
      sampleSize: 4
      successfulSamplesRequired: 3
      additionalLatencyInMilliseconds: 50
    }

    healthProbeSettings: {
      probePath: '/'
      probeRequestType: 'HEAD'
      probeProtocol: 'Http'
      probeIntervalInSeconds: 100
    }

    sessionAffinityState: 'Disabled'
  }
}

resource frontDoorCustomDomain 'Microsoft.Cdn/profiles/customdomains@2021-06-01' = {
  parent: frontDoor
  name: 'vanity-domain'

  properties: {
    hostName: '${parFrontDoorDns}.${parParentDnsName}'
    tlsSettings: {
      certificateType: 'ManagedCertificate'
      minimumTlsVersion: 'TLS12'
    }

    azureDnsZone: {
      id: parentDnsZone.id
    }
  }
}

resource frontDoorOrigin 'Microsoft.Cdn/profiles/origingroups/origins@2021-06-01' = {
  parent: frontDoorOriginGroup
  name: 'default-origin'

  properties: {
    hostName: parOriginHostName
    httpPort: 80
    httpsPort: 443
    originHostHeader: parOriginHostName
    priority: 1
    weight: 1000
    enabledState: 'Enabled'
    enforceCertificateNameCheck: true
  }
}

resource frontDoorRoute 'Microsoft.Cdn/profiles/afdendpoints/routes@2021-06-01' = {
  parent: frontDoorEndpoint
  name: 'default-route'

  properties: {
    customDomains: [
      {
        id: frontDoorCustomDomain.id
      }
    ]

    originGroup: {
      id: frontDoorOriginGroup.id
    }

    ruleSets: []
    supportedProtocols: [
      'Https'
    ]
    patternsToMatch: [
      '/*'
    ]
    forwardingProtocol: 'HttpsOnly'
    linkToDefaultDomain: 'Enabled'
    httpsRedirect: 'Enabled'
    enabledState: 'Enabled'
  }
}

module dns 'dns.bicep' = {
  name: 'dns'
  scope: resourceGroup(parManagementSubscriptionId, parDnsResourceGroupName)

  params: {
    parDns: parFrontDoorDns
    parParentDnsName: parParentDnsName
    parCname: frontDoorEndpoint.properties.hostName
    parCnameValidationToken: frontDoorCustomDomain.properties.validationProperties.validationToken
  }
}

// Outputs
output outFrontDoorId string = frontDoor.properties.frontDoorId
output outFrontDoorHostname string = frontDoorEndpoint.properties.hostName
