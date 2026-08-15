namespace WebAPISuperheroUniverse.Entities.DTOs;

public sealed record ModelRegisterRequest(string Username, string Email, string Password);

public sealed record ModelLoginRequest(string Username, string Password);

public sealed record ModelRefreshRequest(string AccessToken, string RefreshToken);

public sealed record ModelLogoutRequest(string RefreshToken);

/// <summary>
/// Returned by register/login/refresh. Deliberately carries no password hash or internal fields -
/// this is the whole reason DTOs exist separately from the xtUsers entity.
/// </summary>
public sealed record ModelAuthResponse(
    int UserId,
    string Username,
    string Email,
    IReadOnlyList<string> Roles,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken);

public sealed record ModelUserProfileResponse(
    int UserId,
    string Username,
    string Email,
    IReadOnlyList<string> Roles,
    DateTime CreatedAt);
