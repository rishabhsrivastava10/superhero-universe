using Microsoft.EntityFrameworkCore;
using WebAPISuperheroUniverse.DBContext.EntityFramework;
using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;

namespace WebAPISuperheroUniverse.API.Services;

public sealed class DashboardService(AppDbContext db) : IDashboardService
{
    private const int TopHeroCount = 5;
    private const int RecentBattleCount = 6;

    /// <summary>
    /// Flat projection of everything a ranking might sort by, fetched once and reused for the
    /// overall/Marvel/DC tables. A typed record rather than an anonymous type so it can be passed
    /// to the local ranking function without resorting to `dynamic`.
    /// </summary>
    private sealed record ClsRankingRow(
        int Id, string Name, string? RealName, string Universe, string Alignment, string? ImageUrl,
        int PowerLevel, int Strength, int Speed, int Intelligence, int Combat, int Durability,
        int BattlesWon, int TotalBattles);

    public async Task<ModelDashboard> GetDashboardAsync(CancellationToken cancellationToken)
    {
        // Each of these is a separate aggregate query rather than one giant join. Joining
        // unrelated aggregates would multiply rows together and produce wrong counts, and the
        // queries are cheap because they are all COUNT/GROUP BY over indexed columns.
        //
        // The GROUP BY is projected to an anonymous type and ordered BEFORE being mapped to
        // ModelChartSlice: EF Core cannot translate an OrderBy over a property of a
        // constructor-projected record, and fails at runtime rather than compile time.
        var heroesByUniverse = (await db.Superheroes
                .AsNoTracking()
                .GroupBy(h => h.Universe)
                .Select(g => new { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .ToListAsync(cancellationToken))
            .Select(x => new ModelChartSlice(x.Label, x.Value))
            .ToList();

        var heroesByAlignment = (await db.Superheroes
                .AsNoTracking()
                .GroupBy(h => h.Alignment)
                .Select(g => new { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .ToListAsync(cancellationToken))
            .Select(x => new ModelChartSlice(x.Label, x.Value))
            .ToList();

        var missionsByStatus = (await db.Missions
                .AsNoTracking()
                .GroupBy(m => m.Status)
                .Select(g => new { Label = g.Key, Value = g.Count() })
                .ToListAsync(cancellationToken))
            .Select(x => new ModelChartSlice(x.Label, x.Value))
            .ToList();

        var totalHeroes = heroesByUniverse.Sum(s => s.Value);
        var marvel = heroesByUniverse.FirstOrDefault(s => s.Label == "Marvel")?.Value ?? 0;
        var dc = heroesByUniverse.FirstOrDefault(s => s.Label == "DC")?.Value ?? 0;

        var totals = new ModelDashboardTotals(
            Superheroes: totalHeroes,
            MarvelHeroes: marvel,
            DcHeroes: dc,
            // Anything that is neither Marvel nor DC - the schema deliberately allows custom universes.
            OtherUniverseHeroes: totalHeroes - marvel - dc,
            Powers: await db.Powers.CountAsync(cancellationToken),
            Teams: await db.Teams.CountAsync(cancellationToken),
            Battles: await db.Battles.CountAsync(cancellationToken),
            Missions: await db.Missions.CountAsync(cancellationToken),
            MissionsCompleted: missionsByStatus
                .Where(s => s.Label is nameof(EnumMissionStatus.Success) or nameof(EnumMissionStatus.Failed))
                .Sum(s => s.Value),
            MissionsSucceeded: missionsByStatus
                .FirstOrDefault(s => s.Label == nameof(EnumMissionStatus.Success))?.Value ?? 0);

        var topSuperheroes = await db.Superheroes
            .AsNoTracking()
            .OrderByDescending(h => h.PowerLevel)
            .ThenBy(h => h.Id)
            .Take(TopHeroCount)
            .Select(h => new ModelDashboardHero(
                h.Id, h.Name, h.Universe, h.ImageUrl, h.PowerLevel,
                db.Battles.Count(b => b.WinnerId == h.Id),
                db.Battles.Count(b => b.Hero1Id == h.Id || b.Hero2Id == h.Id)))
            .ToListAsync(cancellationToken);

        // Same reason as the GROUP BY queries above: filter and order on the anonymous
        // projection, then map to the DTO once the results are in memory.
        var mostVictorious = (await db.Superheroes
                .AsNoTracking()
                .Select(h => new
                {
                    h.Id, h.Name, h.Universe, h.ImageUrl, h.PowerLevel,
                    BattlesWon = db.Battles.Count(b => b.WinnerId == h.Id),
                    TotalBattles = db.Battles.Count(b => b.Hero1Id == h.Id || b.Hero2Id == h.Id),
                })
                .Where(x => x.BattlesWon > 0)
                .OrderByDescending(x => x.BattlesWon)
                .ThenByDescending(x => x.PowerLevel)
                .Take(TopHeroCount)
                .ToListAsync(cancellationToken))
            .Select(x => new ModelDashboardHero(
                x.Id, x.Name, x.Universe, x.ImageUrl, x.PowerLevel, x.BattlesWon, x.TotalBattles))
            .ToList();

        var largestTeams = (await db.Teams
                .AsNoTracking()
                .Select(t => new
                {
                    t.Id, t.Name, t.Universe,
                    MemberCount = t.SuperheroTeams.Count,
                    TotalPowerLevel = t.SuperheroTeams.Sum(st => (int?)st.Superhero.PowerLevel) ?? 0,
                })
                .OrderByDescending(x => x.MemberCount)
                .ThenByDescending(x => x.TotalPowerLevel)
                .Take(TopHeroCount)
                .ToListAsync(cancellationToken))
            .Select(x => new ModelDashboardTeam(x.Id, x.Name, x.Universe, x.MemberCount, x.TotalPowerLevel))
            .ToList();

        var recentBattles = await db.Battles
            .AsNoTracking()
            .OrderByDescending(b => b.BattleDate)
            .ThenByDescending(b => b.Id)
            .Take(RecentBattleCount)
            .Select(b => new ModelDashboardBattle(
                b.Id,
                b.Hero1.Name, b.Hero1.ImageUrl, b.Hero1Score,
                b.Hero2.Name, b.Hero2.ImageUrl, b.Hero2Score,
                b.Winner != null ? b.Winner.Name : null,
                b.BattleDate))
            .ToListAsync(cancellationToken);

        return new ModelDashboard(
            totals, heroesByUniverse, heroesByAlignment, missionsByStatus,
            topSuperheroes, mostVictorious, largestTeams, recentBattles);
    }

    public async Task<ModelRankings> GetRankingsAsync(string? sortBy, int take, CancellationToken cancellationToken)
    {
        var field = (sortBy ?? "powerlevel").Trim().ToLowerInvariant();
        if (!IDashboardService.AllowedRankingFields.Contains(field))
        {
            field = "powerlevel";
        }

        var safeTake = Math.Clamp(take, 1, 50);

        // Projected once, then ranked three ways in memory. The whole roster is small and this
        // avoids three near-identical round trips; a large roster would want per-universe queries.
        var all = await db.Superheroes
            .AsNoTracking()
            .Select(h => new ClsRankingRow(
                h.Id, h.Name, h.RealName, h.Universe, h.Alignment, h.ImageUrl,
                h.PowerLevel, h.Strength, h.Speed, h.Intelligence, h.Combat, h.Durability,
                db.Battles.Count(b => b.WinnerId == h.Id),
                db.Battles.Count(b => b.Hero1Id == h.Id || b.Hero2Id == h.Id)))
            .ToListAsync(cancellationToken);

        // Switch over an allow-list rather than building a dynamic expression from user input.
        Func<ClsRankingRow, int> valueOf = field switch
        {
            "strength" => h => h.Strength,
            "speed" => h => h.Speed,
            "intelligence" => h => h.Intelligence,
            "combat" => h => h.Combat,
            "durability" => h => h.Durability,
            "wins" => h => h.BattlesWon,
            _ => h => h.PowerLevel,
        };

        List<ModelRankingEntry> Rank(IEnumerable<ClsRankingRow> source) =>
            source
                .Select(h => (Hero: h, Value: valueOf(h)))
                // Id as the final tie-breaker keeps the order stable between requests.
                .OrderByDescending(x => x.Value)
                .ThenByDescending(x => x.Hero.PowerLevel)
                .ThenBy(x => x.Hero.Id)
                .Take(safeTake)
                .Select((x, index) => new ModelRankingEntry(
                    index + 1,
                    x.Hero.Id, x.Hero.Name, x.Hero.RealName, x.Hero.Universe,
                    x.Hero.Alignment, x.Hero.ImageUrl,
                    x.Value, x.Hero.BattlesWon, x.Hero.TotalBattles))
                .ToList();

        return new ModelRankings(
            SortedBy: field,
            Overall: Rank(all),
            Marvel: Rank(all.Where(h => h.Universe == "Marvel")),
            Dc: Rank(all.Where(h => h.Universe == "DC")));
    }
}
