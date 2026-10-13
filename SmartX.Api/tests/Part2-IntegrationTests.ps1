
$ErrorActionPreference = "Stop"

$baseUrl = "https://localhost:7179"
$passed = 0
$failed = 0

function Assert-True {
    param(
        [bool]$Condition,
        [string]$Description
    )

    if ($Condition) {
        Write-Host "PASS: $Description" -ForegroundColor Green
        $script:passed++
    }
    else {
        Write-Host "FAIL: $Description" -ForegroundColor Red
        $script:failed++
    }
}

function Send-Json {
    param(
        [string]$Path,
        [object]$Data
    )

    $body = $Data | ConvertTo-Json -Depth 6

    return Invoke-RestMethod `
        -Uri "$baseUrl$Path" `
        -Method Post `
        -ContentType "application/json" `
        -Body $body
}

Write-Host "Smart-X Part 2 Integration Tests"
Write-Host "================================"

$sensors = @(Invoke-RestMethod "$baseUrl/api/sensors")

if ($sensors.Count -eq 0) {
    throw "Register at least one sensor before running tests."
}

$sensorId = [string]($sensors[0].id)

if ([string]::IsNullOrWhiteSpace($sensorId)) {
    throw "Unable to retrieve a valid sensor ID."
}

Write-Host "Selected sensor ID: $sensorId"
Assert-True `
    ($sensorId -match '^[0-9a-fA-F-]{36}$') `
    "One valid sensor GUID was selected"

$initialQueue = Invoke-RestMethod `
    "$baseUrl/api/telemetry-queue/status"

$initialHistory = Invoke-RestMethod `
    "$baseUrl/api/telemetry-history/count"

$packet = Send-Json "/api/telemetry/float" @{
    sensorId = $sensorId
    metric = "temperature"
    value = 55.0
}

Assert-True `
    ($packet.sensorId -eq $sensorId) `
    "Telemetry accepted through HTTP API"

$queue = Invoke-RestMethod `
    "$baseUrl/api/telemetry-queue/status"

Assert-True `
    ($queue.processedCount -gt $initialQueue.processedCount) `
    "FIFO queue processed submitted telemetry"

$history = Invoke-RestMethod `
    "$baseUrl/api/telemetry-history/count"

Assert-True `
    ($history.count -gt $initialHistory.count) `
    "Sorted telemetry history recorded reading"

$alertsBefore = Invoke-RestMethod `
    "$baseUrl/api/device-alerts/status"

$low = Send-Json "/api/device-alerts" @{
    sensorId = $sensorId
    severity = 3
    message = "Integration test low priority"
}

$critical = Send-Json "/api/device-alerts" @{
    sensorId = $sensorId
    severity = 0
    message = "Integration test critical priority"
}

$processedAlert = Invoke-RestMethod `
    -Uri "$baseUrl/api/device-alerts/process-next" `
    -Method Post

Assert-True `
    ($processedAlert.id -eq $critical.id) `
    "Critical alert processed ahead of low alert"

$issueType = "Error"

$firstIssue = Invoke-RestMethod `
    -Uri "$baseUrl/api/device-issues/$sensorId/$issueType" `
    -Method Post

$secondIssue = Invoke-RestMethod `
    -Uri "$baseUrl/api/device-issues/$sensorId/$issueType" `
    -Method Post

Assert-True `
    (-not $secondIssue.added) `
    "HashSet rejected duplicate active issue"

$command = Send-Json "/api/device-commands" @{
    sensorId = $sensorId
    type = 1
}

Assert-True `
    ($command.status -eq "Completed") `
    "Manual device command executed"

$state = Invoke-RestMethod `
    "$baseUrl/api/device-commands/state/$sensorId"

Assert-True `
    ($state.enabled -eq $false) `
    "Device state updated by command"

$undone = Invoke-RestMethod `
    -Uri "$baseUrl/api/device-commands/undo" `
    -Method Post

Assert-True `
    ($undone.id -eq $command.id -and
     $undone.status -eq "Undone") `
    "Stack undo restored most recent reversible command"

$restoredState = Invoke-RestMethod `
    "$baseUrl/api/device-commands/state/$sensorId"

Assert-True `
    ($restoredState.enabled -eq $true) `
    "Device returned to enabled state"

$search = Send-Json "/api/predictive-insights/search" @{
    sensorId = $sensorId
    query = "integration-test-device"
}

$insights = Invoke-RestMethod `
    "$baseUrl/api/predictive-insights"

Assert-True `
    ($insights.recordedSearchCount -ge 1) `
    "Predictive engine recorded search history"

Assert-True `
    ($insights.analysedSensorCount -ge 1) `
    "Predictive engine analysed registered sensors"

Write-Host ""
Write-Host "Passed: $passed"
Write-Host "Failed: $failed"

if ($failed -gt 0) {
    exit 1
}

exit 0
