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
        $content = $null
        try {
            $reader = New-Object System.IO.StreamReader($resp.GetResponseStream())
            $raw = $reader.ReadToEnd(); $reader.Close()
            if ($raw) { try { $content = $raw | ConvertFrom-Json } catch { $content = $raw } }
        } catch {}
        return @{ Status = [int]$resp.StatusCode; Body = $content }
    } catch { return @{ Status = -1; Body = $_.Exception.Message } }
}

$admin = (Call POST "$root/auth/login" @{ username='admin'; password='Admin@12345' }).Body.accessToken
$u = "battleTester_$(Get-Random -Maximum 999999)"
$plain = (Call POST "$root/auth/register" @{ username=$u; email="$u@example.com"; password='ValidPass123' }).Body.accessToken

$heroes = (Call GET "$root/superheroes?pageSize=100" $null $admin).Body.items
function HeroId($name) { ($heroes | Where-Object { $_.name -eq $name })[0].id }
$superman = HeroId 'Superman'; $batman = HeroId 'Batman'; $thanos = HeroId 'Thanos'
$joker    = HeroId 'Joker';    $thor   = HeroId 'Thor'

Write-Host "`n--- Authorization ---"
Check "anonymous simulate -> 401" ((Call POST "$root/battles/simulate" @{ hero1Id=$superman; hero2Id=$batman } $null).Status -eq 401) "wrong status"
Check "anonymous history -> 401" ((Call GET "$root/battles" $null $null).Status -eq 401) "wrong status"
Check "plain USER can simulate (not admin-only)" ((Call POST "$root/battles/simulate" @{ hero1Id=$superman; hero2Id=$batman } $plain).Status -eq 201) "wrong status"

Write-Host "`n--- Validation ---"
$r = Call POST "$root/battles/simulate" @{ hero1Id=$superman; hero2Id=$superman } $admin
Check "hero cannot battle itself -> 400 (not 500)" ($r.Status -eq 400) "got $($r.Status)"
# The specific reason lives in errors.<Field>, consistent with every other validation failure;
# the Angular error interceptor surfaces the first field error to the user.
$fieldErrors = if ($r.Body.errors) { $r.Body.errors.PSObject.Properties.Value | ForEach-Object { $_ } } else { @() }
Check "self-battle reason given at field level" (($fieldErrors -join ' ') -match 'cannot battle themselves') "errors=$($r.Body.errors | ConvertTo-Json -Compress)"
Check "missing hero -> 404" ((Call POST "$root/battles/simulate" @{ hero1Id=$superman; hero2Id=999999 } $admin).Status -eq 404) "wrong status"
Check "zero id -> 400" ((Call POST "$root/battles/simulate" @{ hero1Id=0; hero2Id=$batman } $admin).Status -eq 400) "wrong status"

Write-Host "`n--- Simulate ---"
$r = Call POST "$root/battles/simulate" @{ hero1Id=$superman; hero2Id=$batman } $admin
Check "simulate -> 201 Created" ($r.Status -eq 201) "got $($r.Status)"
$b = $r.Body
Check "returns a persisted battleId" ($b.battleId -gt 0) "got $($b.battleId)"
Check "both combatants returned" ($b.hero1.name -eq 'Superman' -and $b.hero2.name -eq 'Batman') "got $($b.hero1.name)/$($b.hero2.name)"
Check "scores are within 0-100" ($b.hero1.score -ge 0 -and $b.hero1.score -le 100 -and $b.hero2.score -ge 0 -and $b.hero2.score -le 100) "got $($b.hero1.score)/$($b.hero2.score)"
Check "a winner was decided" ($null -ne $b.winnerId) "null winner"
Check "winner matches the higher score" (
    ($b.hero1.score -gt $b.hero2.score -and $b.winnerId -eq $b.hero1.id) -or
    ($b.hero2.score -gt $b.hero1.score -and $b.winnerId -eq $b.hero2.id) -or
    ($b.hero1.score -eq $b.hero2.score)
) "scores $($b.hero1.score)/$($b.hero2.score) winner=$($b.winnerId)"
Check "loser is the other hero" ($b.loserId -ne $b.winnerId) "winner=$($b.winnerId) loser=$($b.loserId)"
Check "summary is populated" (-not [string]::IsNullOrWhiteSpace($b.summary)) "empty"
Check "summary names the winner first" ($b.summary.StartsWith($b.winnerName)) "summary='$($b.summary)'"
Check "breakdown covers 6 attributes" ($b.breakdown.Count -eq 6) "got $($b.breakdown.Count)"
Check "breakdown weights sum to 100" ((($b.breakdown | Measure-Object -Property weightPercent -Sum).Sum) -eq 100) "got $((($b.breakdown | Measure-Object -Property weightPercent -Sum).Sum))"
$pl = $b.breakdown | Where-Object { $_.attribute -eq 'PowerLevel' }
Check "breakdown reports real attribute values" ($pl.hero1Value -eq 98 -and $pl.hero2Value -eq 78) "got $($pl.hero1Value)/$($pl.hero2Value)"
Check "breakdown marks who won each attribute" ($pl.wonBy -eq 1) "got $($pl.wonBy)"

Write-Host "`n--- Business logic sanity ---"
# Thanos (99 PL) vs Joker (65 PL) is a big enough gap that variance can never flip it.
$thanosWins = 0
for ($i = 0; $i -lt 12; $i++) {
    $x = (Call POST "$root/battles/simulate" @{ hero1Id=$thanos; hero2Id=$joker } $admin).Body
    if ($x.winnerId -eq $thanos) { $thanosWins++ }
}
Check "overwhelming favourite wins every time (12/12)" ($thanosWins -eq 12) "Thanos won $thanosWins/12"

# Superman vs Thor (98 vs 97) is close enough that variance should produce mixed results.
$supermanWins = 0
for ($i = 0; $i -lt 20; $i++) {
    $x = (Call POST "$root/battles/simulate" @{ hero1Id=$superman; hero2Id=$thor } $admin).Body
    if ($x.winnerId -eq $superman) { $supermanWins++ }
}
Check "evenly matched heroes produce varied results (not always same winner)" ($supermanWins -gt 0 -and $supermanWins -lt 20) "Superman won $supermanWins/20"

$scoresA = @(); for ($i=0; $i -lt 8; $i++) { $scoresA += (Call POST "$root/battles/simulate" @{ hero1Id=$superman; hero2Id=$batman } $admin).Body.hero1.score }
Check "repeat matchups do not always give identical scores (variance is live)" (($scoresA | Select-Object -Unique).Count -gt 1) "all scores were $($scoresA[0])"

Write-Host "`n--- History ---"
$r = Call GET "$root/battles?pageSize=5" $null $admin
Check "history -> 200" ($r.Status -eq 200) "got $($r.Status)"
Check "history is paged" ($r.Body.items.Count -le 5 -and $r.Body.totalCount -gt 5) "count=$($r.Body.items.Count) total=$($r.Body.totalCount)"
Check "history newest first" ($r.Body.items[0].battleDate -ge $r.Body.items[1].battleDate) "out of order"
Check "history item carries both hero names" (-not [string]::IsNullOrWhiteSpace($r.Body.items[0].hero1Name) -and -not [string]::IsNullOrWhiteSpace($r.Body.items[0].hero2Name)) "missing names"

$p1 = (Call GET "$root/battles?page=1&pageSize=5" $null $admin).Body.items
$p2 = (Call GET "$root/battles?page=2&pageSize=5" $null $admin).Body.items
$overlap = @($p1 | Where-Object { $p2.id -contains $_.id })
Check "history pages do not overlap" ($overlap.Count -eq 0) "$($overlap.Count) overlapping"

$r = Call GET "$root/battles?superheroId=$joker" $null $admin
$notJoker = @($r.Body.items | Where-Object { $_.hero1Id -ne $joker -and $_.hero2Id -ne $joker })
Check "filter by superheroId works" ($notJoker.Count -eq 0 -and $r.Body.totalCount -gt 0) "$($notJoker.Count) unrelated, total=$($r.Body.totalCount)"

Write-Host "`n--- Fetch a single battle ---"
$r = Call GET "$root/battles/$($b.battleId)" $null $admin
Check "get by id -> 200" ($r.Status -eq 200) "got $($r.Status)"
Check "stored scores are returned UNCHANGED (not re-simulated)" ($r.Body.hero1.score -eq $b.hero1.score -and $r.Body.hero2.score -eq $b.hero2.score) "stored $($b.hero1.score)/$($b.hero2.score), got $($r.Body.hero1.score)/$($r.Body.hero2.score)"
Check "stored winner unchanged" ($r.Body.winnerId -eq $b.winnerId) "got $($r.Body.winnerId)"
Check "missing battle -> 404" ((Call GET "$root/battles/999999" $null $admin).Status -eq 404) "wrong status"

Write-Host "`n--- Referential integrity ---"
Check "hero with battle history cannot be deleted -> 409" ((Call DELETE "$root/superheroes/$joker" $null $admin).Status -eq 409) "wrong status"

Write-Host "`n============================="
Write-Host "PASSED: $($script:pass)   FAILED: $($script:fail)"
Write-Host "============================="

# Machine-readable summary. Write-Host goes to the console only, so the runner cannot capture it -
# this line goes to the output stream, and the exit code lets CI fail the build.
Write-Output "SHU_SUMMARY PASSED=$($script:pass) FAILED=$($script:fail)"
exit $(if ($script:fail -eq 0) { 0 } else { 1 })
