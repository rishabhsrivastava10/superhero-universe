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
$u = "missionTester_$(Get-Random -Maximum 999999)"
$plain = (Call POST "$root/auth/register" @{ username=$u; email="$u@example.com"; password='ValidPass123' }).Body.accessToken

$heroes = (Call GET "$root/superheroes?pageSize=100" $null $admin).Body.items
function HeroId($n) { ($heroes | Where-Object { $_.name -eq $n })[0].id }
$superman = HeroId 'Superman'; $thor = HeroId 'Thor'; $thanos = HeroId 'Thanos'
$darkseid = HeroId 'Darkseid'; $joker = HeroId 'Joker'; $catwoman = HeroId 'Catwoman'
$harley = HeroId 'Harley Quinn'

Write-Host "`n--- Authorization ---"
Check "anonymous list -> 401" ((Call GET "$root/missions" $null $null).Status -eq 401) "wrong status"
Check "user can list -> 200" ((Call GET "$root/missions" $null $plain).Status -eq 200) "wrong status"
Check "user create -> 403 (admin only)" ((Call POST "$root/missions" @{ title='Nope'; difficulty='Easy'; requiredHeroCount=1 } $plain).Status -eq 403) "wrong status"
Check "user reset -> 403 (admin only)" ((Call POST "$root/missions/1/reset" $null $plain).Status -eq 403) "wrong status"

Write-Host "`n--- Read ---"
$r = Call GET "$root/missions" $null $admin
Check "seeded missions returned" ($r.Body.Count -ge 3) "got $($r.Body.Count)"
Check "list reports assigned hero count" ($null -ne ($r.Body[0].assignedHeroCount)) "missing"
$r = Call GET "$root/missions?status=Pending" $null $admin
Check "filter by status" ((@($r.Body | Where-Object { $_.status -ne 'Pending' })).Count -eq 0) "mixed statuses"
Check "missing mission -> 404" ((Call GET "$root/missions/999999" $null $admin).Status -eq 404) "wrong status"

Write-Host "`n--- Admin CRUD ---"
$title = "Test Mission $(Get-Random -Maximum 999999)"
$r = Call POST "$root/missions" @{ title=$title; description='Made by tests'; location='Test City'; difficulty='Hard'; requiredHeroCount=3 } $admin
Check "create -> 201" ($r.Status -eq 201) "got $($r.Status)"
$missionId = $r.Body.id
Check "new mission starts Pending" ($r.Body.status -eq 'Pending') "got $($r.Body.status)"
Check "new mission has no squad" ($r.Body.assignedHeroes.Count -eq 0) "got $($r.Body.assignedHeroes.Count)"
Check "duplicate title -> 409" ((Call POST "$root/missions" @{ title=$title; difficulty='Easy'; requiredHeroCount=1 } $admin).Status -eq 409) "wrong status"
Check "invalid difficulty -> 400" ((Call POST "$root/missions" @{ title="X $title"; difficulty='Impossible'; requiredHeroCount=1 } $admin).Status -eq 400) "wrong status"
Check "zero required heroes -> 400" ((Call POST "$root/missions" @{ title="Y $title"; difficulty='Easy'; requiredHeroCount=0 } $admin).Status -eq 400) "wrong status"
$r = Call PUT "$root/missions/$missionId" @{ title="$title Updated"; description='Edited'; location='Test City'; difficulty='Medium'; requiredHeroCount=2 } $admin
Check "update -> 200" ($r.Status -eq 200) "got $($r.Status)"
Check "update applied" ($r.Body.difficulty -eq 'Medium' -and $r.Body.requiredHeroCount -eq 2) "got $($r.Body.difficulty)/$($r.Body.requiredHeroCount)"

Write-Host "`n--- Starting a mission ---"
Check "no heroes -> 400" ((Call POST "$root/missions/$missionId/start" @{ superheroIds=@() } $admin).Status -eq 400) "wrong status"
Check "unknown hero id -> 404" ((Call POST "$root/missions/$missionId/start" @{ superheroIds=@(999999) } $admin).Status -eq 404) "wrong status"
Check "unknown mission -> 404" ((Call POST "$root/missions/999999/start" @{ superheroIds=@($superman) } $admin).Status -eq 404) "wrong status"

$r = Call POST "$root/missions/$missionId/start" @{ superheroIds=@($superman, $thor) } $plain
Check "plain USER can start a mission" ($r.Status -eq 200) "got $($r.Status)"
$res = $r.Body
Check "result reports Success or Failed" ($res.status -in @('Success','Failed')) "got $($res.status)"
Check "succeeded flag matches status" (($res.succeeded -and $res.status -eq 'Success') -or (-not $res.succeeded -and $res.status -eq 'Failed')) "mismatch"
Check "success chance within 5-95" ($res.successChancePercent -ge 5 -and $res.successChancePercent -le 95) "got $($res.successChancePercent)"
Check "roll within 1-100" ($res.rollPercent -ge 1 -and $res.rollPercent -le 100) "got $($res.rollPercent)"
Check "outcome consistent with roll vs chance" (($res.rollPercent -le $res.successChancePercent) -eq $res.succeeded) "roll=$($res.rollPercent) chance=$($res.successChancePercent) succeeded=$($res.succeeded)"
Check "squad returned" ($res.squad.Count -eq 2) "got $($res.squad.Count)"
Check "factors explain the calculation" ($res.factors.Count -ge 3) "got $($res.factors.Count)"
Check "summary is populated" (-not [string]::IsNullOrWhiteSpace($res.summary)) "empty"

$r = Call GET "$root/missions/$missionId" $null $admin
Check "mission status persisted" ($r.Body.status -in @('Success','Failed')) "got $($r.Body.status)"
Check "squad persisted" ($r.Body.assignedHeroes.Count -eq 2) "got $($r.Body.assignedHeroes.Count)"

Write-Host "`n--- Re-run protection ---"
$r = Call POST "$root/missions/$missionId/start" @{ superheroIds=@($superman) } $admin
Check "restarting a resolved mission -> 409" ($r.Status -eq 409) "got $($r.Status)"
Check "409 explains a reset is needed" ($r.Body.message -match 'reset') "msg='$($r.Body.message)'"

Check "admin reset -> 204" ((Call POST "$root/missions/$missionId/reset" $null $admin).Status -eq 204) "wrong status"
$r = Call GET "$root/missions/$missionId" $null $admin
Check "reset returns mission to Pending" ($r.Body.status -eq 'Pending') "got $($r.Body.status)"
Check "reset clears the squad" ($r.Body.assignedHeroes.Count -eq 0) "got $($r.Body.assignedHeroes.Count)"
Check "mission can be started again after reset" ((Call POST "$root/missions/$missionId/start" @{ superheroIds=@($thor) } $admin).Status -eq 200) "wrong status"

Write-Host "`n--- Business rules ---"
# Hard mission (par 82), 3 required. A single weak hero is both understaffed and underpowered.
$hardId = (Call POST "$root/missions" @{ title="Hard $title"; difficulty='Hard'; requiredHeroCount=3 } $admin).Body.id
$weak = (Call POST "$root/missions/$hardId/start" @{ superheroIds=@($joker) } $admin).Body
Check "weak understaffed squad on a hard mission gets a low chance" ($weak.successChancePercent -le 20) "chance=$($weak.successChancePercent)"

$null = Call POST "$root/missions/$hardId/reset" $null $admin
$strong = (Call POST "$root/missions/$hardId/start" @{ superheroIds=@($superman, $thor, $thanos, $darkseid) } $admin).Body
Check "strong oversized squad on the same mission gets a high chance" ($strong.successChancePercent -ge 80) "chance=$($strong.successChancePercent)"
Check "stronger squad beats weaker squad on identical mission" ($strong.successChancePercent -gt $weak.successChancePercent) "$($strong.successChancePercent) vs $($weak.successChancePercent)"

# Same squad, same mission, but Easy vs Hard should differ.
$easyId = (Call POST "$root/missions" @{ title="Easy $title"; difficulty='Easy'; requiredHeroCount=2 } $admin).Body.id
$hard2Id = (Call POST "$root/missions" @{ title="Hard2 $title"; difficulty='Hard'; requiredHeroCount=2 } $admin).Body.id
$easyRes = (Call POST "$root/missions/$easyId/start" @{ superheroIds=@($catwoman, $harley) } $admin).Body
$hardRes = (Call POST "$root/missions/$hard2Id/start" @{ superheroIds=@($catwoman, $harley) } $admin).Body
Check "identical squad has better odds on Easy than Hard" ($easyRes.successChancePercent -gt $hardRes.successChancePercent) "easy=$($easyRes.successChancePercent) hard=$($hardRes.successChancePercent)"

# Understaffing must matter even with strong heroes.
$u1 = (Call POST "$root/missions" @{ title="Under $title"; difficulty='Medium'; requiredHeroCount=4 } $admin).Body.id
$u2 = (Call POST "$root/missions" @{ title="Full $title"; difficulty='Medium'; requiredHeroCount=1 } $admin).Body.id
$under = (Call POST "$root/missions/$u1/start" @{ superheroIds=@($superman) } $admin).Body
$full  = (Call POST "$root/missions/$u2/start" @{ superheroIds=@($superman) } $admin).Body
Check "understaffed squad is penalised vs a fully staffed one" ($full.successChancePercent -gt $under.successChancePercent) "full=$($full.successChancePercent) under=$($under.successChancePercent)"

# Rolls must vary across attempts.
$rolls = @()
for ($i = 0; $i -lt 10; $i++) {
    $mid = (Call POST "$root/missions" @{ title="Roll $i $title"; difficulty='Medium'; requiredHeroCount=1 } $admin).Body.id
    $rolls += (Call POST "$root/missions/$mid/start" @{ superheroIds=@($superman) } $admin).Body.rollPercent
    $null = Call DELETE "$root/missions/$mid" $null $admin
}
Check "rolls vary between attempts (randomness is live)" (($rolls | Select-Object -Unique).Count -gt 1) "all rolls were $($rolls[0])"

Write-Host "`n--- Cleanup ---"
foreach ($id in @($missionId, $hardId, $easyId, $hard2Id, $u1, $u2)) { $null = Call DELETE "$root/missions/$id" $null $admin }
Check "delete -> 204" ((Call DELETE "$root/missions/999999" $null $admin).Status -eq 404) "wrong status for missing"
Check "hero survived mission deletion" ((Call GET "$root/superheroes/$superman" $null $admin).Status -eq 200) "hero gone!"

Write-Host "`n============================="
Write-Host "PASSED: $($script:pass)   FAILED: $($script:fail)"
Write-Host "============================="

# Machine-readable summary. Write-Host goes to the console only, so the runner cannot capture it -
# this line goes to the output stream, and the exit code lets CI fail the build.
Write-Output "SHU_SUMMARY PASSED=$($script:pass) FAILED=$($script:fail)"
exit $(if ($script:fail -eq 0) { 0 } else { 1 })
