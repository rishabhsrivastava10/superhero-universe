# Runs the published Superhero Universe API standalone, so the Angular app can call it
# without needing `dotnet run` (or Visual Studio) open.
#
# Re-publish after backend changes, from this folder:
#   dotnet publish WebAPISuperheroUniverse\API\WebAPISuperheroUniverse.API.csproj -c Release -o Publish
#
# Stop a running instance with:
#   Get-Process WebAPISuperheroUniverse.API | Stop-Process -Force
$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location "$scriptDir\Publish"
# Development is required for Swagger to be registered (it is gated behind IsDevelopment()).
$env:ASPNETCORE_ENVIRONMENT = "Development"
& ".\WebAPISuperheroUniverse.API.exe" --urls "http://localhost:5024"
