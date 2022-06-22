param (
    $applicationName,
    $appRoles = $null
)

az ad app create --display-name $applicationName --identifier-uris "api://$applicationName"
$applicationId = (az ad app list --filter "displayName eq '$applicationName'" --query '[].appId') | ConvertFrom-Json

$applicationServicePrincipal = az ad sp show --id $applicationId
if ($null -eq $applicationServicePrincipal) {
    az ad sp create --id $applicationId
}

if ($null -ne $appRoles) {
    az ad app update --id $applicationId --app-roles .azure-pipelines/app-registrations/$appRoles
}
