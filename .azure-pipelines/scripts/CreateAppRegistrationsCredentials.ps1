param (
    $environment,
    $location
)

. "./.azure-pipelines/scripts/functions/CreateAppRegistrationCredential.ps1" `
    -keyVaultName "kv-geoloc-$environment-$location" `
    -applicationName "geolocation-lookup-api-$environment" `
    -secretPrefix "geolocation-lookup-api-$environment" `
    -secretDisplayName 'publicwebapp'