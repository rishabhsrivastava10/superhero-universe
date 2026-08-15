$ErrorActionPreference = "Continue"
$base = "$(if ($env:SHU_BASE_URL) { $env:SHU_BASE_URL } else { "http://localhost:5024" })/api/auth"
$script:pass = 0; $script:fail = 0

function Check($name, $condition, $detail) {
    if ($condition) { Write-Host "  PASS  $name" -ForegroundColor Green; $script:pass++ }
    else { Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red; $script:fail++ }
}

# Windows PowerShell 5.1 throws on non-2xx, so the status/body must be dug out of the exception.
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
        $status = [int]$resp.StatusCode
        $content = $null
        try {
            $reader = New-Object System.IO.StreamReader($resp.GetResponseStream())
            $raw = $reader.ReadToEnd(); $reader.Close()
            if ($raw) { try { $content = $raw | ConvertFrom-Json } catch { $content = $raw } }
        } catch {}
        return @{ Status = $status; Body = $content }
    } catch {
        return @{ Status = -1; Body = $_.Exception.Message }
    }
}

$u = "testuser_$(Get-Random -Maximum 999999)"

Write-Host "`n--- Validation ---"
$r = Call POST "$base/register" @{ username = "ab"; email = "not-an-email"; password = "weak" }
Check "weak/invalid registration rejected with 400" ($r.Status -eq 400) "got $($r.Status)"
Check "400 body reports per-field errors" ($null -ne $r.Body.errors -and $r.Body.errors.PSObject.Properties.Name.Count -ge 3) "errors=$($r.Body.errors | ConvertTo-Json -Compress)"

Write-Host "`n--- Register ---"
$r = Call POST "$base/register" @{ username = $u; email = "$u@example.com"; password = "ValidPass123" }
Check "valid registration returns 201" ($r.Status -eq 201) "got $($r.Status): $($r.Body | ConvertTo-Json -Compress)"
$accessToken = $r.Body.accessToken
$refreshToken = $r.Body.refreshToken
Check "registration returns an access token" (-not [string]::IsNullOrWhiteSpace($accessToken)) "missing"
Check "registration returns a refresh token" (-not [string]::IsNullOrWhiteSpace($refreshToken)) "missing"
Check "new user gets the User role" ($r.Body.roles -contains "User") "roles=$($r.Body.roles -join ',')"
Check "new user does NOT get the Admin role" (-not ($r.Body.roles -contains "Admin")) "privilege escalation!"
Check "response contains no password hash" ($null -eq $r.Body.passwordHash) "leaked!"

$r2 = Call POST "$base/register" @{ username = $u; email = "different_$u@example.com"; password = "ValidPass123" }
Check "duplicate username returns 409" ($r2.Status -eq 409) "got $($r2.Status)"
$r3 = Call POST "$base/register" @{ username = "other_$u"; email = "$u@example.com"; password = "ValidPass123" }
Check "duplicate email returns 409" ($r3.Status -eq 409) "got $($r3.Status)"

Write-Host "`n--- Login ---"
$r = Call POST "$base/login" @{ username = $u; password = "WrongPassword1" }
Check "wrong password returns 401" ($r.Status -eq 401) "got $($r.Status)"
$wrongPassMsg = $r.Body.message
$r = Call POST "$base/login" @{ username = "nonexistent_$u"; password = "WrongPassword1" }
Check "unknown username returns 401" ($r.Status -eq 401) "got $($r.Status)"
Check "wrong-password and unknown-user messages are identical (no user enumeration)" ($r.Body.message -eq $wrongPassMsg) "'$($r.Body.message)' vs '$wrongPassMsg'"

$r = Call POST "$base/login" @{ username = $u; password = "ValidPass123" }
Check "correct credentials return 200" ($r.Status -eq 200) "got $($r.Status)"
$accessToken = $r.Body.accessToken
$refreshToken = $r.Body.refreshToken

Write-Host "`n--- Protected endpoints ---"
$r = Call GET "$base/me" $null $null
Check "GET /me without a token returns 401" ($r.Status -eq 401) "got $($r.Status)"
$r = Call GET "$base/me" $null "garbage.token.value"
Check "GET /me with a forged token returns 401" ($r.Status -eq 401) "got $($r.Status)"
$r = Call GET "$base/me" $null $accessToken
Check "GET /me with a valid token returns 200" ($r.Status -eq 200) "got $($r.Status)"
Check "GET /me returns the right user" ($r.Body.username -eq $u) "got '$($r.Body.username)'"
Check "GET /me does not leak the password hash" ($null -eq $r.Body.passwordHash) "leaked!"

Write-Host "`n--- Role-based authorization ---"
$r = Call GET "$base/admin-check" $null $accessToken
Check "non-admin hitting admin endpoint gets 403 (not 401)" ($r.Status -eq 403) "got $($r.Status)"
$r = Call GET "$base/admin-check" $null $null
Check "anonymous hitting admin endpoint gets 401" ($r.Status -eq 401) "got $($r.Status)"

Write-Host "`n--- Refresh token rotation ---"
$r = Call POST "$base/refresh" @{ accessToken = $accessToken; refreshToken = $refreshToken }
Check "refresh returns 200" ($r.Status -eq 200) "got $($r.Status)"
$newRefresh = $r.Body.refreshToken
Check "refresh issues a DIFFERENT refresh token (rotation)" ($newRefresh -ne $refreshToken) "token unchanged"
$r = Call POST "$base/refresh" @{ accessToken = $accessToken; refreshToken = $refreshToken }
Check "reusing the old refresh token returns 401" ($r.Status -eq 401) "got $($r.Status) -- old token still works!"
$r = Call POST "$base/refresh" @{ accessToken = $accessToken; refreshToken = "totally-made-up-token" }
Check "made-up refresh token returns 401" ($r.Status -eq 401) "got $($r.Status)"
$r = Call POST "$base/refresh" @{ accessToken = "forged.access.token"; refreshToken = $newRefresh }
Check "valid refresh token + forged access token returns 401" ($r.Status -eq 401) "got $($r.Status)"

Write-Host "`n--- Logout ---"
$r = Call POST "$base/logout" @{ refreshToken = $newRefresh } $accessToken
Check "logout returns 204" ($r.Status -eq 204) "got $($r.Status)"
$r = Call POST "$base/refresh" @{ accessToken = $accessToken; refreshToken = $newRefresh }
Check "refresh after logout returns 401" ($r.Status -eq 401) "got $($r.Status) -- revoked token still works!"

Write-Host "`n============================="
Write-Host "PASSED: $($script:pass)   FAILED: $($script:fail)"
Write-Host "============================="

# Machine-readable summary. Write-Host goes to the console only, so the runner cannot capture it -
# this line goes to the output stream, and the exit code lets CI fail the build.
Write-Output "SHU_SUMMARY PASSED=$($script:pass) FAILED=$($script:fail)"
exit $(if ($script:fail -eq 0) { 0 } else { 1 })
