namespace WebAPISuperheroUniverse.Entities.Tables;

/// <summary>
/// A server-side refresh token. A JWT access token cannot be revoked before it expires, so the
/// long-lived half of the session is stored here instead, where it CAN be revoked.
/// </summary>
public class ModelRefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Set when the token is used (rotation) or explicitly logged out.</summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>The token issued in its place on rotation - leaves an audit trail of the token chain.</summary>
    public string? ReplacedByToken { get; set; }

    public ModelUser User { get; set; } = null!;

    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;
}
