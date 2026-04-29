$ErrorActionPreference = "Stop"

Set-Location $PSScriptRoot\..
Get-ChildItem . -Recurse | Unblock-File -ErrorAction SilentlyContinue

dotnet tool restore
dotnet restore
dotnet ef database drop -f -s src/SystemManagement.WebApi -p src/SystemManagement.Infrastructure
dotnet ef database update -s src/SystemManagement.WebApi -p src/SystemManagement.Infrastructure
Write-Host "Database migrated successfully." -ForegroundColor Green
