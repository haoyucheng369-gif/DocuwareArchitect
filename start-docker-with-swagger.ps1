param(
    [switch]$Build
)

$composeArgs = if ($Build) { 'up --build -d' } else { 'up -d' }
Write-Host "Starting services with docker compose $composeArgs..."

docker compose $composeArgs

$urls = @(
    'http://localhost:5000/swagger',
    'http://localhost:5001',
    'http://localhost:5002/swagger',
    'http://localhost:8080/admin/master/console/'
)

$timeoutSeconds = 60
$intervalSeconds = 2
$deadline = (Get-Date).AddSeconds($timeoutSeconds)

Write-Host "Waiting for REST API Swagger to become available..."
while ((Get-Date) -lt $deadline) {
    try {
        $response = Invoke-WebRequest -Uri $urls[0] -Method Head -TimeoutSec 5
        if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 400) {
            break
        }
    } catch {
        Start-Sleep -Seconds $intervalSeconds
    }
}

if ((Get-Date) -ge $deadline) {
    Write-Warning "Timeout waiting for $($urls[0]). Services may still be starting."
}

foreach ($url in $urls) {
    Write-Host "Opening $url"
    Start-Process $url
}

Write-Host "Done. If the browser did not open, copy/paste one of the URLs above."
