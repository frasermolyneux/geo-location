param (
    $environment,
    $location
)

$principalId = az account show --query id -o tsv

az keyvault set-policy --name "kv-geoloc-$environment-$location" --spn $principalId --secret-permissions get set