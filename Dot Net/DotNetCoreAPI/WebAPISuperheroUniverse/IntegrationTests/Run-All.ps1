<#
.SYNOPSIS
    Runs every API integration suite against a running instance and reports a combined total.

.DESCRIPTION
    These are black-box tests: they drive the real HTTP API against the real SQL Server database,
    so they cover the whole stack - routing, model binding, validation, authorisation, EF Core
    translation and the database's own constraints. That is deliberately different from the xUnit
    project, which tests business logic in isolation with no I/O.

    Each suite creates the data it needs and removes any users/records it created. The seeded
    demo data (admin user, 28 characters, 8 teams) is left intact.

.PARAMETER BaseUrl
    Where the API is listening. Defaults to the local published instance.

.EXAMPLE
    .\Run-All.ps1
    .\Run-All.ps1 -BaseUrl http://localhost:5024

.NOTES
    Prerequisites:
      * The API must already be running   (..\start-api.ps1)
      * SQL Server must hold the seeded SuperheroUniverseDb
      * The admin account from 12_SeedAdminUser.sql must exist
#>
[CmdletBinding()]
param(
    [string]$BaseUrl = 'http://localhost:5024'
)

$ErrorActionPreference = 'Continue'
$suiteDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# The suites read this, falling back to the local default when it is not set.
$env:SHU_BASE_URL = $BaseUrl

# Fail fast with a useful message rather than 200 confusing connection errors.
try {
    Invoke-WebRequest -Uri "$BaseUrl/swagger/v1/swagger.json" -UseBasicParsing -TimeoutSec 10 | Out-Null
}
catch {
    Write-Host "Cannot reach the API at $BaseUrl" -ForegroundColor Red
    Write-Host "Start it first with: ..\start-api.ps1" -ForegroundColor Yellow
    exit 1
}

$suites = Get-ChildItem -Path $suiteDir -Filter '0*.ps1' | Sort-Object Name
$totalPass = 0
$totalFail = 0
$failedSuites = @()

foreach ($suite in $suites) {
    Write-Host ''
    Write-Host ('=' * 62) -ForegroundColor Cyan
    Write-Host " $($suite.BaseName)" -ForegroundColor Cyan
    Write-Host ('=' * 62) -ForegroundColor Cyan

    # The suites print their per-test results with Write-Host, which goes straight to the console
    # and CANNOT be captured here. That is why each suite also emits a SHU_SUMMARY line on the
    # output stream - that is what this loop reads.
    $output = & $suite.FullName
    $summary = $output | Where-Object { $_ -match '^SHU_SUMMARY PASSED=(\d+) FAILED=(\d+)$' } | Select-Object -Last 1

    if ($summary -match '^SHU_SUMMARY PASSED=(\d+) FAILED=(\d+)$') {
        $suitePass = [int]$Matches[1]
        $suiteFail = [int]$Matches[2]
        $totalPass += $suitePass
        $totalFail += $suiteFail
        if ($suiteFail -gt 0) { $failedSuites += $suite.BaseName }
        Write-Host " -> $suitePass passed, $suiteFail failed" -ForegroundColor $(if ($suiteFail -eq 0) { 'Green' } else { 'Red' })
    }
    else {
        Write-Host "  (no SHU_SUMMARY line found - treating as a failure)" -ForegroundColor Red
        $totalFail++
        $failedSuites += $suite.BaseName
    }
}

Write-Host ''
Write-Host ('=' * 62)
Write-Host " TOTAL   PASSED: $totalPass   FAILED: $totalFail" -ForegroundColor $(if ($totalFail -eq 0) { 'Green' } else { 'Red' })
if ($failedSuites.Count) {
    Write-Host " Failing suites: $($failedSuites -join ', ')" -ForegroundColor Red
}
Write-Host ('=' * 62)

# Non-zero exit so CI treats a failure as a failure.
exit $(if ($totalFail -eq 0) { 0 } else { 1 })
