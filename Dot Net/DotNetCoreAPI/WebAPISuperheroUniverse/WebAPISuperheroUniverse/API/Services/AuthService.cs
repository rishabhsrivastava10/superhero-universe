using System.IdentityModel.Tokens.Jwt;
using CommonAPI.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebAPISuperheroUniverse.DBContext.EntityFramework;
using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;
using WebAPISuperheroUniverse.Entities.Tables;

namespace WebAPISuperheroUniverse.API.Services;

public sealed class AuthService(
    AppDbContext db,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator,
    IOptions<ClsJwtSettings> jwtOptions,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly ClsJwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<(EnumAuthOutcome, ModelAuthResponse?)> RegisterAsync(ModelRegisterRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim();

        // Checked explicitly (rather than relying on the unique constraint to throw) so the caller
        // gets a clear 409 telling them WHICH field collided.
        if (await db.Users.AnyAsync(u => u.Username == username, cancellationToken))
        {
            logger.LogInformation("Registration rejected - username {Username} already taken", username);
            return (EnumAuthOutcome.UsernameTaken, null);
        }

        if (await db.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            logger.LogInformation("Registration rejected - email already registered");
            return (EnumAuthOutcome.EmailTaken, null);
        }

        var user = new ModelUser
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Every new account gets the plain "User" role. Admin is granted deliberately, never by self-registration.
        var userRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == nameof(EnumRoleName.User), cancellationToken);
        if (userRole is not null)
        {
            user.UserRoles.Add(new ModelUserRole { Role = userRole });
        }
        else
        {
            logger.LogWarning("Seeded role {Role} is missing - registering {Username} with no roles", nameof(EnumRoleName.User), username);
        }

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {Username} registered with id {UserId}", username, user.Id);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        return (EnumAuthOutcome.Success, await IssueTokensAsync(user, roles, cancellationToken));
    }

    public async Task<(EnumAuthOutcome, ModelAuthResponse?)> LoginAsync(ModelLoginRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();

        var user = await db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

        // Same outcome whether the user doesn't exist or the password is wrong - telling them
        // apart would let an attacker enumerate valid usernames.
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            logger.LogWarning("Failed login attempt for {Username}", username);
            return (EnumAuthOutcome.InvalidCredentials, null);
        }

        if (!user.IsActive)
        {
            logger.LogWarning("Login blocked - account {Username} is deactivated", username);
            return (EnumAuthOutcome.AccountInactive, null);
        }

        logger.LogInformation("User {Username} logged in", username);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        return (EnumAuthOutcome.Success, await IssueTokensAsync(user, roles, cancellationToken));
    }

    public async Task<(EnumAuthOutcome, ModelAuthResponse?)> RefreshAsync(ModelRefreshRequest request, CancellationToken cancellationToken)
    {
        // The access token is expired by definition here, but its signature must still be valid -
        // this stops a stolen refresh token being redeemed without the matching access token.
        var principal = tokenGenerator.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal is null)
        {
            logger.LogWarning("Refresh rejected - access token failed validation");
            return (EnumAuthOutcome.InvalidRefreshToken, null);
        }

        var subject = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!int.TryParse(subject, out var userId))
        {
            logger.LogWarning("Refresh rejected - access token has no usable subject claim");
            return (EnumAuthOutcome.InvalidRefreshToken, null);
        }

        var storedToken = await db.RefreshTokens
            .Include(rt => rt.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (storedToken is null || storedToken.UserId != userId || !storedToken.IsActive)
        {
            logger.LogWarning("Refresh rejected for user {UserId} - refresh token missing, mismatched or no longer active", userId);
            return (EnumAuthOutcome.InvalidRefreshToken, null);
        }

        if (!storedToken.User.IsActive)
        {
            return (EnumAuthOutcome.AccountInactive, null);
        }

        var roles = storedToken.User.UserRoles.Select(ur => ur.Role.Name).ToList();
        var response = await IssueTokensAsync(storedToken.User, roles, cancellationToken, rotating: storedToken);

        logger.LogInformation("Refresh token rotated for user {UserId}", userId);
        return (EnumAuthOutcome.Success, response);
    }

    public async Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var storedToken = await db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);
        if (storedToken is null || !storedToken.IsActive)
        {
            return false;
        }

        storedToken.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {UserId} logged out - refresh token revoked", storedToken.UserId);
        return true;
    }

    public async Task<ModelUserProfileResponse?> GetProfileAsync(int userId, CancellationToken cancellationToken) =>
        await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new ModelUserProfileResponse(
                u.Id,
                u.Username,
                u.Email,
                u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                u.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Issues a fresh access + refresh token pair. When <paramref name="rotating"/> is supplied,
    /// that token is revoked and chained to the replacement, so a refresh token is single-use.
    /// </summary>
    private async Task<ModelAuthResponse> IssueTokensAsync(
        ModelUser user,
        IReadOnlyList<string> roles,
        CancellationToken cancellationToken,
        ModelRefreshToken? rotating = null)
    {
        var accessToken = tokenGenerator.CreateAccessToken(user.Id, user.Username, user.Email, roles);
        var refreshToken = tokenGenerator.CreateRefreshToken();
        var now = DateTime.UtcNow;

        db.RefreshTokens.Add(new ModelRefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            CreatedAt = now,
            ExpiresAt = now.AddDays(_jwtSettings.RefreshTokenDays)
        });

        if (rotating is not null)
        {
            rotating.RevokedAt = now;
            rotating.ReplacedByToken = refreshToken;
        }

        await db.SaveChangesAsync(cancellationToken);

        return new ModelAuthResponse(
            user.Id,
            user.Username,
            user.Email,
            roles,
            accessToken,
            now.AddMinutes(_jwtSettings.AccessTokenMinutes),
            refreshToken);
    }
}
