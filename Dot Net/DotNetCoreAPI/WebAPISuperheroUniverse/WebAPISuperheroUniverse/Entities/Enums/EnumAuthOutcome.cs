namespace WebAPISuperheroUniverse.Entities.Enums;

/// <summary>
/// Why an auth operation succeeded or failed. Returned instead of throwing, because a failed
/// login is an ordinary expected outcome, not an exceptional condition. The controller maps
/// each value to an HTTP status code.
/// </summary>
public enum EnumAuthOutcome
{
    Success,
    UsernameTaken,
    EmailTaken,
    InvalidCredentials,
    AccountInactive,
    InvalidRefreshToken
}
