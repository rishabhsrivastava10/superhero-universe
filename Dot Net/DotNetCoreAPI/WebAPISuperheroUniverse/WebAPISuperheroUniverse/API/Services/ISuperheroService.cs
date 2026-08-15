using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;

namespace WebAPISuperheroUniverse.API.Services;

public interface ISuperheroService
{
    Task<ModelPagedResponse<ModelSuperheroListItem>> GetPagedAsync(ModelSuperheroQuery query, CancellationToken cancellationToken);

    Task<ModelSuperheroDetail?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<(EnumCrudOutcome Outcome, ModelSuperheroDetail? Superhero)> CreateAsync(ModelSuperheroCreateRequest request, CancellationToken cancellationToken);

    Task<(EnumCrudOutcome Outcome, ModelSuperheroDetail? Superhero)> UpdateAsync(int id, ModelSuperheroUpdateRequest request, CancellationToken cancellationToken);

    Task<EnumCrudOutcome> DeleteAsync(int id, CancellationToken cancellationToken);
}
