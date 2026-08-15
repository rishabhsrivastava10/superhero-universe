using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;

namespace WebAPISuperheroUniverse.API.Services;

public interface IPowerService
{
    Task<IReadOnlyList<ModelPowerListItem>> GetAllAsync(CancellationToken cancellationToken);

    Task<ModelPowerListItem?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<(EnumCrudOutcome Outcome, ModelPowerListItem? Power)> CreateAsync(ModelPowerRequest request, CancellationToken cancellationToken);

    Task<(EnumCrudOutcome Outcome, ModelPowerListItem? Power)> UpdateAsync(int id, ModelPowerRequest request, CancellationToken cancellationToken);

    Task<EnumCrudOutcome> DeleteAsync(int id, CancellationToken cancellationToken);

    /// <summary>Replaces a superhero's entire power set with the supplied ids.</summary>
    Task<(EnumCrudOutcome Outcome, IReadOnlyList<string>? Powers)> AssignToSuperheroAsync(
        int superheroId, ModelAssignPowersRequest request, CancellationToken cancellationToken);
}
