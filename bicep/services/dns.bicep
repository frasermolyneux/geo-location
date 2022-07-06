targetScope = 'resourceGroup'

param parDnsZoneName string
param parParentDnsName string
param parCname string

// Existing Resources
resource parentZone 'Microsoft.Network/dnsZones@2018-05-01' existing = {
  name: parParentDnsName
}

// Module Resources
resource cname 'Microsoft.Network/dnszones/CNAME@2018-05-01' = {
  parent: parentZone
  name: parDnsZoneName
  properties: {
    TTL: 3600
    CNAMERecord: {
      cname: parCname
    }
  }
}
