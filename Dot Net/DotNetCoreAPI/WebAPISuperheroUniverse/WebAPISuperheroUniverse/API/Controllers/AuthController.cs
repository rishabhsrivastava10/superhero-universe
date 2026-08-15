using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPISuperheroUniverse.API.Services;
using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;
using WebAPISuperheroUniverse.Entities.Models;

namespace WebAPISuperheroUniverse.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ModelAuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(ModelRegisterRequest request, CancellationToken cancellationToken)
    {
        var (outcome, response) = await authService.RegisterAsync(request, cancellationToken);

        return outcome switch
        {
            EnumAuthOutcome.Success => Created($"/api/auth/users/{response!.UserId}", response),
            EnumAuthOutcome.UsernameTaken => Conflict(Error(StatusCodes.Status409Conflict, "That username is already taken.")),
            EnumAuthOutcome.EmailTaken => Conflict(Error(StatusCodes.Status409Conflict, "That email is already registered.")),
            _ => BadRequest(Error(StatusCodes.Status400BadRequest, "Registration failed."))
        };
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ModelAuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login(ModelLoginRequest request, CancellationToken cancellationToken)
    {
        var (outcome, response) = await authService.LoginAsync(request, cancellationToken);

        return outcome switch
        {
            EnumAuthOutcome.Success => Ok(response),
            // 403, not 401: the credentials were correct, the account is simply not permitted to sign in.
            EnumAuthOutcome.AccountInactive => StatusCode(StatusCodes.Status403Forbidden,
                Error(StatusCodes.Status403Forbidden, "This account has been deactivated.")),
            _ => Unauthorized(Error(StatusCodes.Status401Unauthorized, "Invalid username or password."))
        };
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ModelAuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(ModelRefreshRequest request, CancellationToken cancellationToken)
    {
        var (outcome, response) = await authService.RefreshAsync(request, cancellationToken);

        return outcome switch
        {
            EnumAuthOutcome.Success => Ok(response),
            EnumAuthOutcome.AccountInactive => StatusCode(StatusCodes.Status403Forbidden,
                Error(StatusCodes.Status403Forbidden, "This account has been deactivated.")),
            _ => Unauthorized(Error(StatusCodes.Status401Unauthorized, "Invalid or expired refresh token."))
        };
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] ModelLogoutRequest request, CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(request.RefreshToken, cancellationToken);

        // 204 regardless of whether the token existed - a caller shouldn't be able to probe
        // which refresh tokens are valid, and "already logged out" is not an error worth surfacing.
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ModelUserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ModelErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var subject = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(subject, out var userId))
        {
            return Unauthorized(Error(StatusCodes.Status401Unauthorized, "Token does not identify a valid user."));
        }

        var profile = await authService.GetProfileAsync(userId, cancellationToken);

        return profile is null
            ? NotFound(Error(StatusCodes.Status404NotFound, "User not found."))
            : Ok(profile);
    }

    /// <summary>Temporary probe proving role-based authorization works. Replaced by real admin endpoints in Phase 6.</summary>
    [HttpGet("admin-check")]
    [Authorize(Roles = nameof(EnumRoleName.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult AdminCheck() => Ok(new { message = "You are an admin." });

    private static ModelErrorResponse Error(int statusCode, string message) => new()
    {
        StatusCode = statusCode,
        Message = message,
        Timestamp = DateTime.UtcNow
    };
}
