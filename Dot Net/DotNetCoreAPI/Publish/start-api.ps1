# Runs the published Superhero Universe API standalone (no `dotnet run`/rebuild needed).
# Re-publish after backend changes with:
#   dotnet publish WebAPISuperheroUniverse/WebAPISuperheroUniverse.csproj -c Release -o Publish/WebAPISuperheroUniverse
# (run from Dot Net/DotNetCoreAPI)
$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location "$scriptDir\WebAPISuperheroUniverse"
$env:ASPNETCORE_ENVIRONMENT = "Development"
& ".\WebAPISuperheroUniverse.exe" --urls "http://localhost:5024"
