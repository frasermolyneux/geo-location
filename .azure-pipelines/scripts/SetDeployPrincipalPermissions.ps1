param (
    $environment,
    $location
)

$principalId = az ad signed-in-user show --query id -o tsv

az keyvault set-policy --name "kv-geoloc-$environment-$location" --spn $principalId --secret-permissions get update