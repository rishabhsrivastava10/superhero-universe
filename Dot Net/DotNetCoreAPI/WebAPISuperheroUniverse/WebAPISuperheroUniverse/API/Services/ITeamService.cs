using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;

namespace WebAPISuperheroUniverse.API.Services;

public interface ITeamService
{
    Task<IReadOnlyList<ModelTeamListItem>> GetAllAsync(string? universe, CancellationToken cancellationToken);

    Task<ModelTeamDetail?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<(EnumCrudOutcome Outcome, ModelTeamDetail? Team)> CreateAsync(ModelTeamRequest request, CancellationToken cancellationToken);

    Task<(EnumCrudOutcome Outcome, ModelTeamDetail? Team)> UpdateAsync(int id, ModelTeamRequest request, CancellationToken cancellationToken);

    Task<EnumCrudOutcome> DeleteAsync(int id, CancellationToken cancellationToken);

    Task<EnumCrudOutcome> AddMemberAsync(int teamId, int superheroId, CancellationToken cancellationToken);

    Task<EnumCrudOutcome> RemoveMemberAsync(int teamId, int superheroId, CancellationToken cancellationToken);
}
