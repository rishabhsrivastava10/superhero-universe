using WebAPISuperheroUniverse.Entities.DTOs;

namespace WebAPISuperheroUniverse.API.Services;

public interface IDashboardService
{
    Task<ModelDashboard> GetDashboardAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Rankings sorted by one of the allowed attributes, split into overall / Marvel / DC.
    /// </summary>
    Task<ModelRankings> GetRankingsAsync(string? sortBy, int take, CancellationToken cancellationToken);

    /// <summary>Attribute names a ranking may be sorted by.</summary>
    static readonly string[] AllowedRankingFields =
        ["powerlevel", "strength", "speed", "intelligence", "combat", "durability", "wins"];
}
