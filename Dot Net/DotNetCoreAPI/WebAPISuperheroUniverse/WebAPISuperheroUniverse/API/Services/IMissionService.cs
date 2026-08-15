using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;

namespace WebAPISuperheroUniverse.API.Services;

public interface IMissionService
{
    Task<IReadOnlyList<ModelMissionListItem>> GetAllAsync(string? status, CancellationToken cancellationToken);

    Task<ModelMissionDetail?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<(EnumCrudOutcome Outcome, ModelMissionDetail? Mission)> CreateAsync(ModelMissionRequest request, CancellationToken cancellationToken);

    Task<(EnumCrudOutcome Outcome, ModelMissionDetail? Mission)> UpdateAsync(int id, ModelMissionRequest request, CancellationToken cancellationToken);

    Task<EnumCrudOutcome> DeleteAsync(int id, CancellationToken cancellationToken);

    Task<(EnumMissionOutcome Outcome, ModelMissionResult? Result)> StartAsync(
        int id, ModelStartMissionRequest request, CancellationToken cancellationToken);

    /// <summary>Returns a resolved mission to Pending so it can be attempted again.</summary>
    Task<EnumCrudOutcome> ResetAsync(int id, CancellationToken cancellationToken);
}
