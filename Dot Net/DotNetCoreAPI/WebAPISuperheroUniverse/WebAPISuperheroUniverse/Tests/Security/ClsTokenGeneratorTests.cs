using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CommonAPI.Security;
using Microsoft.Extensions.Options;

namespace WebAPISuperheroUniverse.Tests.Security;

public class ClsTokenGeneratorTests
{
    private const string SigningKey = "test-signing-key-that-is-definitely-long-enough-32";

    private static ClsJwtSettings Settings(int accessTokenMinutes = 15) => new()
    {
        Key = SigningKey,
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        AccessTokenMinutes = accessTokenMinutes,
        RefreshTokenDays = 7,
    };

    private static ClsTokenGenerator Generator(ClsJwtSettings? settings = null) =>
        new(Options.Create(settings ?? Settings()));

    [Fact]
    public void CreateAccessToken_ProducesAThreePartJwt()
    {
        var token = Generator().CreateAccessToken(1, "tester", "tester@example.com", ["User"]);

        Assert.Equal(3, token.Split('.').Length);
    }

    [Fact]
    public void CreateAccessToken_EmbedsTheUsersIdentityAndRoles()
    {
        var token = Generator().CreateAccessToken(42, "tester", "tester@example.com", ["User", "Admin"]);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("42", jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("tester", jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value);
        Assert.Equal("tester@example.com", jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Email).Value);

        // Role claims are written with the outbound short name "role" by the handler.
        var roles = jwt.Claims.Where(c => c.Type is "role" or ClaimTypes.Role).Select(c => c.Value).ToList();
        Assert.Contains("User", roles);
        Assert.Contains("Admin", roles);
    }

    [Fact]
    public void CreateAccessToken_NeverContainsThePasswordOrSigningKey()
    {
        var token = Generator().CreateAccessToken(1, "tester", "tester@example.com", ["User"]);

        // A JWT payload is only base64-encoded, not encrypted - anything put in it is public.
        Assert.DoesNotContain(SigningKey, token);
    }

    [Fact]
    public void CreateAccessToken_GivesEveryTokenAUniqueJti()
    {
        var generator = Generator();
        var handler = new JwtSecurityTokenHandler();

        var first = handler.ReadJwtToken(generator.CreateAccessToken(1, "a", "a@x.com", ["User"]));
        var second = handler.ReadJwtToken(generator.CreateAccessToken(1, "a", "a@x.com", ["User"]));

        Assert.NotEqual(
            first.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Jti).Value,
            second.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Jti).Value);
    }

    [Fact]
    public void CreateAccessToken_SetsTheConfiguredExpiry()
    {
        var token = Generator(Settings(accessTokenMinutes: 30)).CreateAccessToken(1, "a", "a@x.com", ["User"]);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Allow a small window for execution time between issuing and asserting.
        var expectedExpiry = DateTime.UtcNow.AddMinutes(30);
        Assert.InRange(jwt.ValidTo, expectedExpiry.AddMinutes(-1), expectedExpiry.AddMinutes(1));
    }

    [Fact]
    public void CreateRefreshToken_ProducesAUniqueHighEntropyValue()
    {
        var generator = Generator();

        var tokens = Enumerable.Range(0, 500).Select(_ => generator.CreateRefreshToken()).ToList();

        Assert.Equal(tokens.Count, tokens.Distinct().Count());
        // 64 random bytes, base64-encoded - long enough that guessing is not feasible.
        Assert.All(tokens, t => Assert.True(t.Length >= 80, $"refresh token too short: {t.Length}"));
    }

    /// <summary>
    /// Builds a token that expired in the past but is otherwise authentic.
    ///
    /// This cannot be produced via CreateAccessToken: that method always sets notBefore to now,
    /// so a negative lifetime would put expiry before notBefore and JwtSecurityToken rejects it.
    /// Constructing it here is the only way to exercise the refresh path honestly.
    /// </summary>
    private static string ExpiredToken(int userId, ClsJwtSettings settings)
    {
        var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(settings.Key));

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: [new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())],
            notBefore: DateTime.UtcNow.AddHours(-2),
            expires: DateTime.UtcNow.AddHours(-1),
            signingCredentials: new Microsoft.IdentityModel.Tokens.SigningCredentials(
                key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ReadsAnExpiredButAuthenticToken()
    {
        // The refresh flow depends on this: the access token is expired by definition, but its
        // signature must still be honoured.
        var principal = Generator().GetPrincipalFromExpiredToken(ExpiredToken(7, Settings()));

        Assert.NotNull(principal);
        // Regression guard: MapInboundClaims defaults to true and silently rewrites "sub" to
        // ClaimTypes.NameIdentifier, which broke the entire refresh flow until it was disabled.
        Assert.Equal("7", principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_StillRejectsAnExpiredTokenSignedWithTheWrongKey()
    {
        // Skipping lifetime validation must not mean skipping signature validation.
        var otherSettings = Settings();
        otherSettings.Key = "a-completely-different-key-also-long-enough-32ch";

        Assert.Null(Generator().GetPrincipalFromExpiredToken(ExpiredToken(7, otherSettings)));
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_RejectsATokenSignedWithADifferentKey()
    {
        var otherSettings = Settings();
        otherSettings.Key = "a-completely-different-key-also-long-enough-32ch";
        var foreignToken = Generator(otherSettings).CreateAccessToken(1, "a", "a@x.com", ["User"]);

        Assert.Null(Generator().GetPrincipalFromExpiredToken(foreignToken));
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_RejectsAWrongIssuerOrAudience()
    {
        var wrongIssuer = Settings();
        wrongIssuer.Issuer = "SomeoneElse";
        var token = Generator(wrongIssuer).CreateAccessToken(1, "a", "a@x.com", ["User"]);

        Assert.Null(Generator().GetPrincipalFromExpiredToken(token));
    }

    [Theory]
    [InlineData("garbage")]
    [InlineData("not.a.jwt")]
    [InlineData("")]
    public void GetPrincipalFromExpiredToken_ReturnsNullRatherThanThrowing_ForMalformedInput(string token)
    {
        Assert.Null(Generator().GetPrincipalFromExpiredToken(token));
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_RejectsATamperedPayload()
    {
        var token = Generator().CreateAccessToken(1, "tester", "tester@example.com", ["User"]);
        var parts = token.Split('.');

        // Swap in a different payload while keeping the original signature.
        var forgedPayload = Convert.ToBase64String("""{"sub":"999","role":"Admin"}"""u8.ToArray())
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var forged = $"{parts[0]}.{forgedPayload}.{parts[2]}";

        Assert.Null(Generator().GetPrincipalFromExpiredToken(forged));
    }
}
