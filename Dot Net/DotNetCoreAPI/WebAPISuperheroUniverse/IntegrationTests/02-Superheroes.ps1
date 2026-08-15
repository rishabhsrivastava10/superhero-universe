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
        if ($body) { $params["Body"] = ($body | ConvertTo-Json -Compress) }
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

# --- tokens -------------------------------------------------------------------------------
$admin = (Call POST "$root/auth/login" @{ username='admin'; password='Admin@12345' }).Body.accessToken
$u = "heroTester_$(Get-Random -Maximum 999999)"
$plain = (Call POST "$root/auth/register" @{ username=$u; email="$u@example.com"; password='ValidPass123' }).Body.accessToken

Write-Host "`n--- Authorization ---"
$r = Call GET "$root/superheroes" $null $null
Check "anonymous list -> 401" ($r.Status -eq 401) "got $($r.Status)"
$r = Call GET "$root/superheroes" $null $plain
Check "authenticated user can list -> 200" ($r.Status -eq 200) "got $($r.Status)"
$r = Call POST "$root/superheroes" @{ name='Nope'; universe='DC'; alignment='Hero'; powerLevel=1; intelligence=1; strength=1; speed=1; durability=1; combat=1 } $plain
Check "non-admin create -> 403" ($r.Status -eq 403) "got $($r.Status)"
$r = Call DELETE "$root/superheroes/1" $null $plain
Check "non-admin delete -> 403" ($r.Status -eq 403) "got $($r.Status)"

Write-Host "`n--- Paging ---"
$r = Call GET "$root/superheroes?page=1&pageSize=3" $null $admin
Check "pageSize honoured (3 items)" ($r.Body.items.Count -eq 3) "got $($r.Body.items.Count)"
$unfiltered = (Call GET "$root/superheroes?pageSize=1" $null $admin).Body.totalCount
Check "totalCount reflects the whole table" ($r.Body.totalCount -eq $unfiltered) "paged=$($r.Body.totalCount) unfiltered=$unfiltered"
$expectedPages = [math]::Ceiling($r.Body.totalCount / 3)
Check "totalPages derived from totalCount and pageSize" ($r.Body.totalPages -eq $expectedPages) "got $($r.Body.totalPages) expected $expectedPages"
Check "hasNextPage true on page 1" ($r.Body.hasNextPage -eq $true) "got $($r.Body.hasNextPage)"
Check "hasPreviousPage false on page 1" ($r.Body.hasPreviousPage -eq $false) "got $($r.Body.hasPreviousPage)"
$p1 = (Call GET "$root/superheroes?page=1&pageSize=5&sortBy=powerLevel&sortDir=desc" $null $admin).Body.items
$p2 = (Call GET "$root/superheroes?page=2&pageSize=5&sortBy=powerLevel&sortDir=desc" $null $admin).Body.items
$overlap = @($p1 | Where-Object { $p2.id -contains $_.id })
Check "pages do not overlap (stable ordering)" ($overlap.Count -eq 0) "$($overlap.Count) hero(es) on both pages"

Write-Host "`n--- Search / filter ---"
$r = Call GET "$root/superheroes?search=bat" $null $admin
Check "search 'bat' finds Batman" (@($r.Body.items | Where-Object { $_.name -eq 'Batman' }).Count -eq 1) "got $($r.Body.items.name -join ',')"
$r = Call GET "$root/superheroes?search=BAT" $null $admin
Check "search is case-insensitive" (@($r.Body.items | Where-Object { $_.name -eq 'Batman' }).Count -eq 1) "got $($r.Body.items.name -join ',')"
$r = Call GET "$root/superheroes?search=wayne" $null $admin
Check "search also matches realName" (@($r.Body.items | Where-Object { $_.name -eq 'Batman' }).Count -eq 1) "got $($r.Body.items.name -join ',')"
$r = Call GET "$root/superheroes?universe=DC" $null $admin
Check "filter universe=DC returns only DC" (($r.Body.items | Where-Object { $_.universe -ne 'DC' }).Count -eq 0) "mixed universes"
$r = Call GET "$root/superheroes?alignment=Villain" $null $admin
Check "filter alignment=Villain returns only villains" (($r.Body.items | Where-Object { $_.alignment -ne 'Villain' }).Count -eq 0) "mixed alignments"
$r = Call GET "$root/superheroes?minPowerLevel=90" $null $admin
Check "minPowerLevel=90 respected" (($r.Body.items | Where-Object { $_.powerLevel -lt 90 }).Count -eq 0) "found below 90"
$r = Call GET "$root/superheroes?search=bat&universe=DC&alignment=Hero&minPowerLevel=70" $null $admin
Check "combined filters work together" ($r.Body.totalCount -eq 1 -and $r.Body.items[0].name -eq 'Batman') "count=$($r.Body.totalCount)"

Write-Host "`n--- Sorting ---"
$r = Call GET "$root/superheroes?sortBy=powerLevel&sortDir=desc&pageSize=100" $null $admin
$levels = $r.Body.items.powerLevel
Check "sort powerLevel desc is ordered" ((($levels | Sort-Object -Descending) -join ',') -eq ($levels -join ',')) "got $($levels -join ',')"
$maxPower = ($r.Body.items | Measure-Object -Property powerLevel -Maximum).Maximum
Check "strongest hero is listed first" ($r.Body.items[0].powerLevel -eq $maxPower) "first=$($r.Body.items[0].powerLevel) max=$maxPower"
$r = Call GET "$root/superheroes?sortBy=name&sortDir=asc&pageSize=100" $null $admin
$names = $r.Body.items.name
Check "sort name asc is ordered" ((($names | Sort-Object) -join ',') -eq ($names -join ',')) "got $($names -join ',')"

Write-Host "`n--- Validation ---"
$r = Call GET "$root/superheroes?sortBy=powerLevel; DROP TABLE xtSuperheroes--" $null $admin
Check "unknown sortBy rejected with 400 (allow-list)" ($r.Status -eq 400) "got $($r.Status)"
$r = Call GET "$root/superheroes?pageSize=99999" $null $admin
Check "excessive pageSize rejected with 400" ($r.Status -eq 400) "got $($r.Status)"
$r = Call GET "$root/superheroes?minPowerLevel=90&maxPowerLevel=10" $null $admin
Check "min > max rejected with 400" ($r.Status -eq 400) "got $($r.Status)"
$r = Call POST "$root/superheroes" @{ name=''; universe=''; alignment='Sidekick'; powerLevel=500; intelligence=-5; strength=1; speed=1; durability=1; combat=1 } $admin
Check "invalid create rejected with 400" ($r.Status -eq 400) "got $($r.Status)"
Check "400 lists the offending fields" ($r.Body.errors.PSObject.Properties.Name.Count -ge 4) "errors=$($r.Body.errors | ConvertTo-Json -Compress)"

Write-Host "`n--- Detail ---"
$r = Call GET "$root/superheroes/999999" $null $admin
Check "missing hero -> 404" ($r.Status -eq 404) "got $($r.Status)"
$supermanId = (Call GET "$root/superheroes?search=Superman" $null $admin).Body.items[0].id
$r = Call GET "$root/superheroes/$supermanId" $null $admin
Check "detail returns 200" ($r.Status -eq 200) "got $($r.Status)"
$listPowers = ((Call GET "$root/superheroes?search=Superman" $null $admin).Body.items[0]).powers
Check "detail powers agree with the list endpoint" (($r.Body.powers | Sort-Object) -join   -eq ($listPowers | Sort-Object) -join  ) "detail=$($r.Body.powers -join  ) list=$($listPowers -join  )"
Check "detail includes at least one power" ($r.Body.powers.Count -gt 0) "got $($r.Body.powers.Count)"
Check "detail includes teams" ($r.Body.teams -contains 'Justice League') "got $($r.Body.teams -join ',')"
Check "detail includes battle stats" ($r.Body.totalBattles -ge 1 -and $r.Body.battlesWon -ge 1) "total=$($r.Body.totalBattles) won=$($r.Body.battlesWon)"

Write-Host "`n--- Admin CRUD ---"
$newName = "Test Hero $(Get-Random -Maximum 999999)"
$r = Call POST "$root/superheroes" @{ name=$newName; realName='Tester'; universe='Indie'; alignment='Anti-Hero'; description='Created by tests'; powerLevel=55; intelligence=60; strength=50; speed=45; durability=40; combat=65; imageUrl=$null } $admin
Check "admin create -> 201" ($r.Status -eq 201) "got $($r.Status): $($r.Body | ConvertTo-Json -Compress)"
$newId = $r.Body.id
Check "created hero echoes its values" ($r.Body.name -eq $newName -and $r.Body.alignment -eq 'Anti-Hero') "got $($r.Body.name)/$($r.Body.alignment)"
Check "custom universe accepted (not hardcoded to Marvel/DC)" ($r.Body.universe -eq 'Indie') "got $($r.Body.universe)"

$r = Call POST "$root/superheroes" @{ name=$newName; universe='DC'; alignment='Hero'; powerLevel=1; intelligence=1; strength=1; speed=1; durability=1; combat=1 } $admin
Check "duplicate name -> 409" ($r.Status -eq 409) "got $($r.Status)"

$r = Call PUT "$root/superheroes/$newId" @{ name="$newName Updated"; realName='Tester'; universe='Indie'; alignment='Hero'; description='Updated'; powerLevel=77; intelligence=60; strength=50; speed=45; durability=40; combat=65; imageUrl=$null } $admin
Check "admin update -> 200" ($r.Status -eq 200) "got $($r.Status)"
Check "update applied" ($r.Body.powerLevel -eq 77 -and $r.Body.alignment -eq 'Hero') "got PL=$($r.Body.powerLevel)"
Check "updatedAt is set" ($null -ne $r.Body.updatedAt) "null"

$r = Call PUT "$root/superheroes/$newId" @{ name="$newName Updated"; universe='Indie'; alignment='Hero'; powerLevel=77; intelligence=60; strength=50; speed=45; durability=40; combat=65 } $admin
Check "renaming a hero to its own name is allowed" ($r.Status -eq 200) "got $($r.Status)"

$r = Call PUT "$root/superheroes/999999" @{ name='Ghost'; universe='DC'; alignment='Hero'; powerLevel=1; intelligence=1; strength=1; speed=1; durability=1; combat=1 } $admin
Check "update missing hero -> 404" ($r.Status -eq 404) "got $($r.Status)"

$r = Call DELETE "$root/superheroes/$newId" $null $admin
Check "admin delete -> 204" ($r.Status -eq 204) "got $($r.Status)"
$r = Call GET "$root/superheroes/$newId" $null $admin
Check "deleted hero is gone -> 404" ($r.Status -eq 404) "got $($r.Status)"
$r = Call DELETE "$root/superheroes/999999" $null $admin
Check "delete missing hero -> 404" ($r.Status -eq 404) "got $($r.Status)"

Write-Host "`n--- Referential integrity ---"
$r = Call DELETE "$root/superheroes/$supermanId" $null $admin
Check "deleting hero with battle history -> 409 (not 500)" ($r.Status -eq 409) "got $($r.Status)"
$r = Call GET "$root/superheroes/$supermanId" $null $admin
Check "that hero still exists after blocked delete" ($r.Status -eq 200) "got $($r.Status)"

Write-Host "`n============================="
Write-Host "PASSED: $($script:pass)   FAILED: $($script:fail)"
Write-Host "============================="

# Machine-readable summary. Write-Host goes to the console only, so the runner cannot capture it -
# this line goes to the output stream, and the exit code lets CI fail the build.
Write-Output "SHU_SUMMARY PASSED=$($script:pass) FAILED=$($script:fail)"
exit $(if ($script:fail -eq 0) { 0 } else { 1 })
