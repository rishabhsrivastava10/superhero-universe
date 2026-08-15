using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;

namespace WebAPISuperheroUniverse.API.Services;

public interface IBattleService
{
    Task<(EnumBattleOutcome Outcome, ModelBattleResult? Result)> SimulateAsync(
        ModelSimulateBattleRequest request, CancellationToken cancellationToken);

    Task<ModelPagedResponse<ModelBattleListItem>> GetHistoryAsync(
        int page, int pageSize, int? superheroId, CancellationToken cancellationToken);

    Task<ModelBattleResult?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
