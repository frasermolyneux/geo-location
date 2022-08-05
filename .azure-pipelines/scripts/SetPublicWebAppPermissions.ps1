param (
    $environment,
    $location
)

$webApp = (az webapp show --name "webapp-geolocation-public-$environment-$location" --resource-group "rg-platform-webapps-$environment-uksouth") | ConvertFrom-Json
$principalId = $webApp.identity.principalId

. "./.azure-pipelines/scripts/functions/GrantLookupApiPermissionsToApp.ps1" -principalId $principalId -environment $environment