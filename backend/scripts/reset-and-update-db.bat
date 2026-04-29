@echo off
cd /d %~dp0\..

dotnet tool restore
if errorlevel 1 exit /b 1

dotnet restore
if errorlevel 1 exit /b 1

dotnet ef database drop -f -s src/SystemManagement.WebApi -p src/SystemManagement.Infrastructure
if errorlevel 1 exit /b 1

dotnet ef database update -s src/SystemManagement.WebApi -p src/SystemManagement.Infrastructure
