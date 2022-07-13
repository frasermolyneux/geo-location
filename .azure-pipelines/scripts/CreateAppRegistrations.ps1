param (
    $environment,
    $location
)

. "./.azure-pipelines/scripts/functions/CreateAppRegistration.ps1" `
    -applicationName "geolocation-lookup-api-$environment" `
    -appRoles "lookup-api-approles.json"
