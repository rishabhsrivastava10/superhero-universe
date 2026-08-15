using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CommonAPI.Security;

public sealed class ClsTokenGenerator(IOptions<ClsJwtSettings> jwtOptions) : ITokenGenerator
{
    private readonly ClsJwtSettings _settings = jwtOptions.Value;

    public string CreateAccessToken(int userId, string username, string email, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            // "sub" is the standard subject claim - the user's stable identifier.
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(JwtRegisteredClaimNames.Email, email),
            // "jti" gives every token a unique id, which is what makes per-token revocation possible later.
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // One claim per role - [Authorize(Roles = "Admin")] matches against these.
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateRefreshToken()
    {
        // Opaque random value, not a JWT - it carries no claims and is only meaningful when
        // matched against the copy stored in xtRefreshTokens.
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _settings.Issuer,
            ValidateAudience = true,
            ValidAudience = _settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
            // The whole point of this method is to read a token that has already expired,
            // so lifetime validation is intentionally skipped. Everything else is still enforced -
            // a forged or tampered token is still rejected.
            ValidateLifetime = false
        };

        try
        {
            // MapInboundClaims = false is essential here. Left at its default of true, the handler
            // rewrites standard JWT claim names into legacy WS-Federation URIs - "sub" silently
            // becomes ClaimTypes.NameIdentifier - and looking up "sub" afterwards returns null.
            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
            var principal = handler.ValidateToken(accessToken, validationParameters, out var securityToken);

            // Guard against a token signed with a weaker/different algorithm than we issue.
            if (securityToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            return null;
        }
    }
}
