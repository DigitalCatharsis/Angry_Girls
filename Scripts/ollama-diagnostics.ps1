$ErrorActionPreference = "Continue"

Write-Host "`n=== Ollama version ===" -ForegroundColor Cyan
ollama --version

Write-Host "`n=== Installed models ===" -ForegroundColor Cyan
ollama list

Write-Host "`n=== Models currently loaded ===" -ForegroundColor Cyan
ollama ps

Write-Host "`n=== Ollama tags API (localhost:11434) ===" -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "http://localhost:11434/api/tags" -Method Get -TimeoutSec 10
    $response.models | Select-Object name, size, modified_at | Format-Table -AutoSize
}
catch {
    Write-Host "Could not query Ollama API: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host "`n=== Runtime environment ===" -ForegroundColor Cyan
Write-Host "Computer: $env:COMPUTERNAME"
Write-Host "OS: $([System.Environment]::OSVersion.VersionString)"
Write-Host "PowerShell: $($PSVersionTable.PSVersion)"
Write-Host "`nThis script only reads status; it does not change models or configuration."
