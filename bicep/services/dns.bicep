targetScope = 'resourceGroup'

param parDnsZoneName string
param parParentDnsName string
param parCname string
param parCnameValidationToken string

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

resource authRecord 'Microsoft.Network/dnszones/TXT@2018-05-01' = {
  parent: parentZone
  name: '_dnsauth.${parDnsZoneName}.${parParentDnsName}'
  properties: {
    TTL: 3600
    TXTRecords: [
      {
        value: [
          parCnameValidationToken
        ]
      }
    ]
    targetResource: {
    }
  }
}
