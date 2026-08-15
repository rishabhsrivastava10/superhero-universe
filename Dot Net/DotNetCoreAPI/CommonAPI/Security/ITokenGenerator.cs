using System.Security.Claims;

namespace CommonAPI.Security;

public interface ITokenGenerator
{
    /// <summary>Creates a signed, short-lived JWT access token carrying the user's identity and roles.</summary>
    string CreateAccessToken(int userId, string username, string email, IEnumerable<string> roles);

    /// <summary>Creates a cryptographically random opaque refresh token. Store its hash/value server-side so it can be revoked.</summary>
    string CreateRefreshToken();

    /// <summary>Reads the principal out of an EXPIRED access token (signature still validated). Used by the refresh flow.</summary>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken);
}
