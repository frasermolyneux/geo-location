targetScope = 'resourceGroup'

param parDnsZoneName string
param parManagementSubscriptionId string
param parManagementResourceGroupName string
param parParentDnsName string

// Existing Resources
resource parentZone 'Microsoft.Network/dnsZones@2018-05-01' existing = {
  name: parParentDnsName
  scope: resourceGroup(parManagementSubscriptionId, parManagementResourceGroupName)
}

// Module Resources
resource parentZoneNs 'Microsoft.Network/dnsZones/NS@2018-05-01' = {
  name: '${parDnsZoneName}/${parParentDnsName}'

  properties: {
    TTL: 3600
    NSRecords: [
      {
        nsdname: 'ns1-09.azure-dns.com.'
      }
      {
        nsdname: 'ns2-09.azure-dns.net.'
      }
      {
        nsdname: 'ns3-09.azure-dns.org.'
      }
      {
        nsdname: 'ns4-09.azure-dns.info.'
      }
    ]
  }
}

resource zone 'Microsoft.Network/dnsZones@2018-05-01' = {
  name: parDnsZoneName
  location: 'global'

  properties: {
    zoneType: 'Public'
  }
}
