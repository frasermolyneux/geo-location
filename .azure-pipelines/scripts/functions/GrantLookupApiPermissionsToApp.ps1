param (
    $principalId,
    $environment
)

$appName = "geolocation-lookup-api-$environment"
$appId = (az ad app list --filter "displayName eq '$appName'" --query '[].appId') | ConvertFrom-Json
$appSpnId = (az ad sp list --filter "appId eq '$appId'" --query '[0].objectId') | ConvertFrom-Json

$permissions = (az rest -m GET -u https://graph.microsoft.com/v1.0/servicePrincipals/$principalId/appRoleAssignments) | ConvertFrom-Json
if ($null -eq ($permissions.value | Where-Object { $_.appRoleId -eq 'b4b62713-44f8-4871-8c10-2c85369b776d' })) {
    az rest -m POST -u https://graph.microsoft.com/v1.0/servicePrincipals/$principalId/appRoleAssignments -b "{'principalId': '$principalId', 'resourceId': '$appSpnId','appRoleId': 'b4b62713-44f8-4871-8c10-2c85369b776d'}"
}