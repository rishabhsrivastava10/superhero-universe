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
$u = "ptTester_$(Get-Random -Maximum 999999)"
$plain = (Call POST "$root/auth/register" @{ username=$u; email="$u@example.com"; password='ValidPass123' }).Body.accessToken

Write-Host "`n=============== POWERS ==============="
Write-Host "`n--- Authorization ---"
Check "anonymous list -> 401" ((Call GET "$root/powers" $null $null).Status -eq 401) "wrong status"
Check "user can list -> 200" ((Call GET "$root/powers" $null $plain).Status -eq 200) "wrong status"
Check "user create -> 403" ((Call POST "$root/powers" @{ name='Nope' } $plain).Status -eq 403) "wrong status"

Write-Host "`n--- Read ---"
$r = Call GET "$root/powers" $null $admin
Check "seeded powers returned" ($r.Body.Count -gt 0) "got $($r.Body.Count)"
Check "powers sorted by name" ((($r.Body.name | Sort-Object) -join ',') -eq ($r.Body.name -join ',')) "not sorted"
$flight = $r.Body | Where-Object { $_.name -eq 'Flight' }
# Derive the expected count from the heroes themselves rather than trusting a fixed number.
$flightHeroes = @((Call GET "$root/superheroes?pageSize=100" $null $admin).Body.items | Where-Object { $_.powers -contains 'Flight' })
Check "superheroCount matches the heroes who actually have that power" ($flight.superheroCount -eq $flightHeroes.Count) "power says $($flight.superheroCount), heroes say $($flightHeroes.Count)"

Write-Host "`n--- Admin CRUD ---"
$pname = "Test Power $(Get-Random -Maximum 999999)"
$r = Call POST "$root/powers" @{ name=$pname; description='Made by tests' } $admin
Check "create -> 201" ($r.Status -eq 201) "got $($r.Status)"
$newPowerId = $r.Body.id
Check "new power has 0 superheroes" ($r.Body.superheroCount -eq 0) "got $($r.Body.superheroCount)"
Check "duplicate name -> 409" ((Call POST "$root/powers" @{ name=$pname } $admin).Status -eq 409) "wrong status"
Check "empty name -> 400" ((Call POST "$root/powers" @{ name='' } $admin).Status -eq 400) "wrong status"
$r = Call PUT "$root/powers/$newPowerId" @{ name="$pname Updated"; description='Edited' } $admin
Check "update -> 200" ($r.Status -eq 200) "got $($r.Status)"
Check "update applied" ($r.Body.description -eq 'Edited') "got $($r.Body.description)"
Check "update missing -> 404" ((Call PUT "$root/powers/999999" @{ name='Ghost' } $admin).Status -eq 404) "wrong status"

Write-Host "`n--- Delete guard ---"
$flightId = $flight.id
Check "delete power in use -> 409" ((Call DELETE "$root/powers/$flightId" $null $admin).Status -eq 409) "wrong status"
Check "power still exists after blocked delete" ((Call GET "$root/powers/$flightId" $null $admin).Status -eq 200) "gone!"
Check "delete unused power -> 204" ((Call DELETE "$root/powers/$newPowerId" $null $admin).Status -eq 204) "wrong status"

Write-Host "`n--- Assigning powers to a hero ---"
$batman = (Call GET "$root/superheroes?search=Batman" $null $admin).Body.items[0]
$origPowers = (Call GET "$root/superheroes/$($batman.id)" $null $admin).Body.powers
Check "Batman starts with 3 powers" ($origPowers.Count -eq 3) "got $($origPowers.Count)"

$allPowers = (Call GET "$root/powers" $null $admin).Body
$flightId2 = ($allPowers | Where-Object { $_.name -eq 'Flight' }).id
$magicId  = ($allPowers | Where-Object { $_.name -eq 'Magic' }).id

$r = Call PUT "$root/superheroes/$($batman.id)/powers" @{ powerIds = @($flightId2, $magicId) } $admin
Check "assign powers -> 200" ($r.Status -eq 200) "got $($r.Status)"
Check "assignment REPLACES the set (now 2)" ($r.Body.Count -eq 2) "got $($r.Body.Count)"
$after = (Call GET "$root/superheroes/$($batman.id)" $null $admin).Body.powers
Check "detail reflects new powers" (($after -contains 'Flight') -and ($after -contains 'Magic')) "got $($after -join ',')"
Check "old powers were removed" (-not ($after -contains 'Weapons Mastery')) "old power still there"

$r = Call PUT "$root/superheroes/$($batman.id)/powers" @{ powerIds = @($flightId2, $flightId2) } $admin
Check "duplicate ids in request tolerated" ($r.Status -eq 200 -and $r.Body.Count -eq 1) "status=$($r.Status) count=$($r.Body.Count)"

Check "invalid power id -> 404" ((Call PUT "$root/superheroes/$($batman.id)/powers" @{ powerIds = @(999999) } $admin).Status -eq 404) "wrong status"
Check "assign to missing hero -> 404" ((Call PUT "$root/superheroes/999999/powers" @{ powerIds = @($flightId2) } $admin).Status -eq 404) "wrong status"
Check "non-admin assign -> 403" ((Call PUT "$root/superheroes/$($batman.id)/powers" @{ powerIds = @($flightId2) } $plain).Status -eq 403) "wrong status"

$r = Call PUT "$root/superheroes/$($batman.id)/powers" @{ powerIds = @() } $admin
Check "empty array clears all powers" ($r.Status -eq 200 -and $r.Body.Count -eq 0) "status=$($r.Status) count=$($r.Body.Count)"

# restore Batman's original powers
$restoreIds = @()
foreach ($n in $origPowers) { $restoreIds += ($allPowers | Where-Object { $_.name -eq $n }).id }
$r = Call PUT "$root/superheroes/$($batman.id)/powers" @{ powerIds = $restoreIds } $admin
Check "Batman's original powers restored" ($r.Body.Count -eq 3) "got $($r.Body.Count)"

Write-Host "`n=============== TEAMS ==============="
Write-Host "`n--- Read ---"
$r = Call GET "$root/teams" $null $admin
Check "seeded teams returned" ($r.Body.Count -gt 0) "got $($r.Body.Count)"
$jl = $r.Body | Where-Object { $_.name -eq 'Justice League' }
$jlDetail = (Call GET "$root/teams/$($jl.id)" $null $admin).Body
Check "list memberCount matches the detail member list" ($jl.memberCount -eq $jlDetail.members.Count) "list=$($jl.memberCount) detail=$($jlDetail.members.Count)"
$r = Call GET "$root/teams?universe=DC" $null $admin
Check "filter by universe" ((@($r.Body | Where-Object { $_.universe -ne 'DC' })).Count -eq 0) "mixed universes"

$r = Call GET "$root/teams/$($jl.id)" $null $admin
Check "team detail -> 200" ($r.Status -eq 200) "got $($r.Status)"
Check "detail lists members" ($r.Body.members.Count -gt 0) "got $($r.Body.members.Count)"
Check "members sorted strongest first" ($r.Body.members[0].name -eq 'Superman') "got $($r.Body.members[0].name)"
Check "statistics: memberCount agrees with the member list" ($r.Body.statistics.memberCount -eq $r.Body.members.Count) "stats=$($r.Body.statistics.memberCount) list=$($r.Body.members.Count)"
Check "statistics: strongest member" ($r.Body.statistics.strongestMemberName -eq 'Superman') "got $($r.Body.statistics.strongestMemberName)"
$expectedTotal = ($r.Body.members | Measure-Object -Property powerLevel -Sum).Sum
Check "statistics: totalPowerLevel correct" ($r.Body.statistics.totalPowerLevel -eq $expectedTotal) "got $($r.Body.statistics.totalPowerLevel), expected $expectedTotal"
$actualHeroes = @($r.Body.members | Where-Object { $_.alignment -eq 'Hero' }).Count
$breakdownTotal = $r.Body.statistics.heroCount + $r.Body.statistics.villainCount + $r.Body.statistics.antiHeroCount
Check "statistics: alignment breakdown matches the members" ($r.Body.statistics.heroCount -eq $actualHeroes) "stats=$($r.Body.statistics.heroCount) actual=$actualHeroes"
Check "statistics: alignment counts sum to the member count" ($breakdownTotal -eq $r.Body.members.Count) "sum=$breakdownTotal members=$($r.Body.members.Count)"
Check "missing team -> 404" ((Call GET "$root/teams/999999" $null $admin).Status -eq 404) "wrong status"

Write-Host "`n--- Admin CRUD ---"
$tname = "Test Team $(Get-Random -Maximum 999999)"
$r = Call POST "$root/teams" @{ name=$tname; universe='Indie'; description='Made by tests'; foundedDate='2020-01-15' } $admin
Check "create -> 201" ($r.Status -eq 201) "got $($r.Status)"
$newTeamId = $r.Body.id
Check "new team has 0 members" ($r.Body.statistics.memberCount -eq 0) "got $($r.Body.statistics.memberCount)"
Check "empty-team stats do not divide by zero" ($r.Body.statistics.averagePowerLevel -eq 0) "got $($r.Body.statistics.averagePowerLevel)"
Check "duplicate name -> 409" ((Call POST "$root/teams" @{ name=$tname; universe='DC' } $admin).Status -eq 409) "wrong status"
Check "future foundedDate -> 400" ((Call POST "$root/teams" @{ name="Future $tname"; universe='DC'; foundedDate='2099-01-01' } $admin).Status -eq 400) "wrong status"
Check "user create team -> 403" ((Call POST "$root/teams" @{ name="X $tname"; universe='DC' } $plain).Status -eq 403) "wrong status"

$r = Call PUT "$root/teams/$newTeamId" @{ name="$tname Updated"; universe='Indie'; description='Edited' } $admin
Check "update -> 200" ($r.Status -eq 200) "got $($r.Status)"
Check "update applied" ($r.Body.description -eq 'Edited') "got $($r.Body.description)"

Write-Host "`n--- Membership ---"
$joker = (Call GET "$root/superheroes?search=Joker" $null $admin).Body.items[0]
Check "add member -> 204" ((Call POST "$root/teams/$newTeamId/members" @{ superheroId=$joker.id } $admin).Status -eq 204) "wrong status"
$r = Call GET "$root/teams/$newTeamId" $null $admin
Check "member appears in team" ($r.Body.members.Count -eq 1 -and $r.Body.members[0].name -eq 'Joker') "got $($r.Body.members.name -join ',')"
Check "villain counted in stats" ($r.Body.statistics.villainCount -eq 1) "got $($r.Body.statistics.villainCount)"
Check "joinedDate recorded" ($null -ne $r.Body.members[0].joinedDate) "null"
Check "duplicate member -> 409" ((Call POST "$root/teams/$newTeamId/members" @{ superheroId=$joker.id } $admin).Status -eq 409) "wrong status"
Check "add missing hero -> 404" ((Call POST "$root/teams/$newTeamId/members" @{ superheroId=999999 } $admin).Status -eq 404) "wrong status"
Check "user add member -> 403" ((Call POST "$root/teams/$newTeamId/members" @{ superheroId=$joker.id } $plain).Status -eq 403) "wrong status"

Check "hero's detail shows the team" (((Call GET "$root/superheroes/$($joker.id)" $null $admin).Body.teams -contains "$tname Updated")) "team missing from hero"

Check "remove member -> 204" ((Call DELETE "$root/teams/$newTeamId/members/$($joker.id)" $null $admin).Status -eq 204) "wrong status"
Check "team is empty again" (((Call GET "$root/teams/$newTeamId" $null $admin).Body.members.Count -eq 0)) "still has members"
Check "remove non-member -> 404" ((Call DELETE "$root/teams/$newTeamId/members/$($joker.id)" $null $admin).Status -eq 404) "wrong status"
Check "hero survived team removal" ((Call GET "$root/superheroes/$($joker.id)" $null $admin).Status -eq 200) "hero deleted!"

Write-Host "`n--- Cleanup ---"
Check "delete team -> 204" ((Call DELETE "$root/teams/$newTeamId" $null $admin).Status -eq 204) "wrong status"
Check "delete missing team -> 404" ((Call DELETE "$root/teams/999999" $null $admin).Status -eq 404) "wrong status"

Write-Host "`n============================="
Write-Host "PASSED: $($script:pass)   FAILED: $($script:fail)"
Write-Host "============================="

# Machine-readable summary. Write-Host goes to the console only, so the runner cannot capture it -
# this line goes to the output stream, and the exit code lets CI fail the build.
Write-Output "SHU_SUMMARY PASSED=$($script:pass) FAILED=$($script:fail)"
exit $(if ($script:fail -eq 0) { 0 } else { 1 })
