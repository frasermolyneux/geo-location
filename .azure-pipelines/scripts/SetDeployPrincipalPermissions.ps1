param (
    $environment,
    $location
)

az keyvault set-policy --name "kv-geoloc-$environment-$location" --spn $env:servicePrincipalId --secret-permissions get set