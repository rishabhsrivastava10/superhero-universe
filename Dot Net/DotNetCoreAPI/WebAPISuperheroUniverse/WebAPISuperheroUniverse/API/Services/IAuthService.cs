using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;

namespace WebAPISuperheroUniverse.API.Services;

public interface IAuthService
{
    Task<(EnumAuthOutcome Outcome, ModelAuthResponse? Response)> RegisterAsync(ModelRegisterRequest request, CancellationToken cancellationToken);

    Task<(EnumAuthOutcome Outcome, ModelAuthResponse? Response)> LoginAsync(ModelLoginRequest request, CancellationToken cancellationToken);

    Task<(EnumAuthOutcome Outcome, ModelAuthResponse? Response)> RefreshAsync(ModelRefreshRequest request, CancellationToken cancellationToken);

    Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken);

    Task<ModelUserProfileResponse?> GetProfileAsync(int userId, CancellationToken cancellationToken);
}
