namespace CommonAPI.Security;

/// <summary>
/// Strongly-typed JWT configuration, bound from the "Jwt" configuration section.
/// Kept in CommonAPI so every API project in this solution family can reuse it.
/// </summary>
public sealed class ClsJwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>Signing key. Never commit a real value - supply it via user secrets or an environment variable.</summary>
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    /// <summary>Access tokens are deliberately short-lived; a stolen one cannot be revoked before it expires.</summary>
    public int AccessTokenMinutes { get; set; } = 15;

    /// <summary>Refresh tokens are long-lived but are stored server-side, so they can be revoked.</summary>
    public int RefreshTokenDays { get; set; } = 7;
}
