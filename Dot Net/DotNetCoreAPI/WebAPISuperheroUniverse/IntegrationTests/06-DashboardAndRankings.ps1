$ErrorActionPreference = "Continue"
$root = "$(if ($env:SHU_BASE_URL) { $env:SHU_BASE_URL } else { "http://localhost:5024" })/api"
$script:pass = 0; $script:fail = 0

function Check($name, $condition, $detail) {
    if ($condition) { Write-Host "  PASS  $name" -ForegroundColor Green; $script:pass++ }
    else { Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red; $script:fail++ }
}

function Call($method, $url, $body, $token) {
    $headers = @{}
    if ($token) { $headers["Authorization"] = "Bearer $token" }
    try {
        $params = @{ Method = $method; Uri = $url; Headers = $headers; ContentType = "application/json"; UseBasicParsing = $true }
        if ($null -ne $body) { $params["Body"] = ($body | ConvertTo-Json -Compress -Depth 6) }
        $r = Invoke-WebRequest @params
        $content = $null
        if ($r.Content) { try { $content = $r.Content | ConvertFrom-Json } catch { $content = $r.Content } }
        return @{ Status = [int]$r.StatusCode; Body = $content }
    } catch [System.Net.WebException] {
        $resp = $_.Exception.Response
        if ($null -eq $resp) { return @{ Status = -1; Body = $_.Exception.Message } }
        return @{ Status = [int]$resp.StatusCode; Body = $null }
    } catch { return @{ Status = -1; Body = $_.Exception.Message } }
}

$admin = (Call POST "$root/auth/login" @{ username='admin'; password='Admin@12345' }).Body.accessToken
$u = "dashTester_$(Get-Random -Maximum 999999)"
$plain = (Call POST "$root/auth/register" @{ username=$u; email="$u@example.com"; password='ValidPass123' }).Body.accessToken

Write-Host "`n--- Authorization ---"
Check "anonymous dashboard -> 401" ((Call GET "$root/dashboard" $null $null).Status -eq 401) "wrong status"
Check "anonymous rankings -> 401" ((Call GET "$root/rankings" $null $null).Status -eq 401) "wrong status"
Check "plain user can read dashboard" ((Call GET "$root/dashboard" $null $plain).Status -eq 200) "wrong status"
Check "plain user can read rankings" ((Call GET "$root/rankings" $null $plain).Status -eq 200) "wrong status"

Write-Host "`n--- Dashboard totals cross-checked against the real endpoints ---"
$d = (Call GET "$root/dashboard" $null $admin).Body
$heroTotal   = (Call GET "$root/superheroes?pageSize=1" $null $admin).Body.totalCount
$powerTotal  = (Call GET "$root/powers" $null $admin).Body.Count
$teamTotal   = (Call GET "$root/teams" $null $admin).Body.Count
$battleTotal = (Call GET "$root/battles?pageSize=1" $null $admin).Body.totalCount
$missionTotal= (Call GET "$root/missions" $null $admin).Body.Count

Check "superhero total matches /superheroes" ($d.totals.superheroes -eq $heroTotal) "dash=$($d.totals.superheroes) actual=$heroTotal"
Check "power total matches /powers" ($d.totals.powers -eq $powerTotal) "dash=$($d.totals.powers) actual=$powerTotal"
Check "team total matches /teams" ($d.totals.teams -eq $teamTotal) "dash=$($d.totals.teams) actual=$teamTotal"
Check "battle total matches /battles" ($d.totals.battles -eq $battleTotal) "dash=$($d.totals.battles) actual=$battleTotal"
Check "mission total matches /missions" ($d.totals.missions -eq $missionTotal) "dash=$($d.totals.missions) actual=$missionTotal"

$marvelActual = (Call GET "$root/superheroes?universe=Marvel&pageSize=1" $null $admin).Body.totalCount
$dcActual     = (Call GET "$root/superheroes?universe=DC&pageSize=1" $null $admin).Body.totalCount
Check "Marvel count matches a filtered query" ($d.totals.marvelHeroes -eq $marvelActual) "dash=$($d.totals.marvelHeroes) actual=$marvelActual"
Check "DC count matches a filtered query" ($d.totals.dcHeroes -eq $dcActual) "dash=$($d.totals.dcHeroes) actual=$dcActual"
Check "marvel + dc + other = total" (($d.totals.marvelHeroes + $d.totals.dcHeroes + $d.totals.otherUniverseHeroes) -eq $d.totals.superheroes) "does not add up"

Write-Host "`n--- Chart data ---"
Check "heroesByUniverse sums to the hero total" ((($d.heroesByUniverse | Measure-Object -Property value -Sum).Sum) -eq $heroTotal) "sum mismatch"
Check "heroesByAlignment sums to the hero total" ((($d.heroesByAlignment | Measure-Object -Property value -Sum).Sum) -eq $heroTotal) "sum mismatch"
Check "heroesByUniverse sorted descending" (
    ($d.heroesByUniverse.value -join ',') -eq (($d.heroesByUniverse.value | Sort-Object -Descending) -join ',')
) "not sorted"
Check "missionsByStatus sums to the mission total" ((($d.missionsByStatus | Measure-Object -Property value -Sum).Sum) -eq $missionTotal) "sum mismatch"

Write-Host "`n--- Top lists ---"
Check "topSuperheroes returns 5" ($d.topSuperheroes.Count -eq 5) "got $($d.topSuperheroes.Count)"
Check "topSuperheroes sorted by power desc" (
    ($d.topSuperheroes.powerLevel -join ',') -eq (($d.topSuperheroes.powerLevel | Sort-Object -Descending) -join ',')
) "not sorted"
$actualTop = (Call GET "$root/superheroes?sortBy=powerLevel&sortDir=desc&pageSize=1" $null $admin).Body.items[0]
Check "strongest hero agrees with /superheroes" ($d.topSuperheroes[0].name -eq $actualTop.name) "dash=$($d.topSuperheroes[0].name) actual=$($actualTop.name)"

Check "mostVictorious sorted by wins desc" (
    ($d.mostVictorious.battlesWon -join ',') -eq (($d.mostVictorious.battlesWon | Sort-Object -Descending) -join ',')
) "not sorted"
Check "mostVictorious excludes heroes with zero wins" ((@($d.mostVictorious | Where-Object { $_.battlesWon -eq 0 })).Count -eq 0) "zero-win hero present"
Check "battlesWon never exceeds totalBattles" ((@($d.mostVictorious | Where-Object { $_.battlesWon -gt $_.totalBattles })).Count -eq 0) "impossible record"

Check "largestTeams sorted by member count desc" (
    ($d.largestTeams.memberCount -join ',') -eq (($d.largestTeams.memberCount | Sort-Object -Descending) -join ',')
) "not sorted"
$jl = (Call GET "$root/teams" $null $admin).Body | Where-Object { $_.name -eq 'Avengers' }
$dashAvengers = $d.largestTeams | Where-Object { $_.name -eq 'Avengers' }
Check "team member count agrees with /teams" ($dashAvengers.memberCount -eq $jl.memberCount) "dash=$($dashAvengers.memberCount) actual=$($jl.memberCount)"

Check "recentBattles newest first" ($d.recentBattles[0].battleDate -ge $d.recentBattles[1].battleDate) "out of order"

Write-Host "`n--- Rankings ---"
$r = (Call GET "$root/rankings?sortBy=powerLevel&take=10" $null $admin).Body
Check "returns overall, marvel and dc" (($null -ne $r.overall) -and ($null -ne $r.marvel) -and ($null -ne $r.dc)) "missing scope"
Check "take honoured" ($r.overall.Count -eq 10) "got $($r.overall.Count)"
# NOTE: $r.overall.rank does NOT work - .Rank is an intrinsic array property (number of
# dimensions), so it shadows the members' own rank. Project explicitly instead.
$ranks = @($r.overall | ForEach-Object { $_.rank })
Check "ranks are 1..n in order" (($ranks -join ',') -eq ((1..$r.overall.Count) -join ',')) "got $($ranks -join ',')"
Check "overall sorted by value desc" (
    ($r.overall.value -join ',') -eq (($r.overall.value | Sort-Object -Descending) -join ',')
) "not sorted"
Check "marvel scope contains only Marvel" ((@($r.marvel | Where-Object { $_.universe -ne 'Marvel' })).Count -eq 0) "mixed"
Check "dc scope contains only DC" ((@($r.dc | Where-Object { $_.universe -ne 'DC' })).Count -eq 0) "mixed"

$byInt = (Call GET "$root/rankings?sortBy=intelligence&take=5" $null $admin).Body
Check "sortBy=intelligence changes the ordering" ($byInt.sortedBy -eq 'intelligence') "got $($byInt.sortedBy)"
Check "intelligence ranking sorted desc" (
    ($byInt.overall.value -join ',') -eq (($byInt.overall.value | Sort-Object -Descending) -join ',')
) "not sorted"
$topIntActual = (Call GET "$root/superheroes?sortBy=intelligence&sortDir=desc&pageSize=1" $null $admin).Body.items[0]
Check "top by intelligence agrees with /superheroes" ($byInt.overall[0].value -eq 100) "got $($byInt.overall[0].value)"

$byWins = (Call GET "$root/rankings?sortBy=wins&take=5" $null $admin).Body
Check "sortBy=wins works" ($byWins.sortedBy -eq 'wins') "got $($byWins.sortedBy)"
Check "wins ranking value equals battlesWon" ($byWins.overall[0].value -eq $byWins.overall[0].battlesWon) "value=$($byWins.overall[0].value) won=$($byWins.overall[0].battlesWon)"

Write-Host "`n--- Input safety ---"
$bad = (Call GET "$root/rankings?sortBy=powerLevel; DROP TABLE xtSuperheroes--" $null $admin)
Check "unknown sortBy falls back safely" ($bad.Status -eq 200 -and $bad.Body.sortedBy -eq 'powerlevel') "status=$($bad.Status) sortedBy=$($bad.Body.sortedBy)"
Check "table survived the injection attempt" ((Call GET "$root/superheroes?pageSize=1" $null $admin).Body.totalCount -eq $heroTotal) "roster changed!"
$big = (Call GET "$root/rankings?take=9999" $null $admin).Body
Check "excessive take is clamped" ($big.overall.Count -le 50) "got $($big.overall.Count)"

Write-Host "`n============================="
Write-Host "PASSED: $($script:pass)   FAILED: $($script:fail)"
Write-Host "============================="

# Machine-readable summary. Write-Host goes to the console only, so the runner cannot capture it -
# this line goes to the output stream, and the exit code lets CI fail the build.
Write-Output "SHU_SUMMARY PASSED=$($script:pass) FAILED=$($script:fail)"
exit $(if ($script:fail -eq 0) { 0 } else { 1 })
