param(
    [string] $deploymentName
)

# Check if there is an existing deployment running
$deploymentOutput = (az deployment sub show --name $deploymentName) | ConvertFrom-Json
$deploymentStatus = $deploymentOutput.properties.provisioningState

while ($deploymentStatus -eq "Running") {
    Write-Host "An existing deployment is running..."
    Start-Sleep -s 10
    $deploymentOutput = (az deployment sub show --name $deploymentName) | ConvertFrom-Json
    $deploymentStatus = $deploymentOutput.properties.provisioningState
}
