param(
    [string] $deploymentName,
    [string] $resourceGroupName
)

# Check if the resource group exists
$resourceGroupExists = (az group exists --name $resourceGroupName) | ConvertFrom-Json

if ($resourceGroupExists -eq $false) {
    Write-Host "Resource group '$resourceGroupName' does not exist."
    return
}

# Check if there is an existing deployment running
$deploymentOutput = (az deployment group show --name $deploymentName --resource-group $resourceGroupName) | ConvertFrom-Json
$deploymentStatus = $deploymentOutput.properties.provisioningState

while ($deploymentStatus -eq "Running") {
    Write-Host "An existing deployment is running..."
    Start-Sleep -s 10
    $deploymentOutput = (az deployment group show --name $deploymentName --resource-group $resourceGroupName) | ConvertFrom-Json
    $deploymentStatus = $deploymentOutput.properties.provisioningState
}
