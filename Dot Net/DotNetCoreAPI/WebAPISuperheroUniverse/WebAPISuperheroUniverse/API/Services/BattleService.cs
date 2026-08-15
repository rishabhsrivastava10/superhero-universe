using Microsoft.EntityFrameworkCore;
using WebAPISuperheroUniverse.API.Class;
using WebAPISuperheroUniverse.DBContext.EntityFramework;
using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;
using WebAPISuperheroUniverse.Entities.Tables;

namespace WebAPISuperheroUniverse.API.Services;

public sealed class BattleService(
    AppDbContext db,
    ClsBattleCalculator calculator,
    ILogger<BattleService> logger) : IBattleService
{
    private const int MaxPageSize = 100;

    public async Task<(EnumBattleOutcome, ModelBattleResult?)> SimulateAsync(
        ModelSimulateBattleRequest request, CancellationToken cancellationToken)
    {
        // Checked before touching the database - a hero fighting itself is nonsense, and the
        // CK_xtBattles_DistinctHeroes constraint would reject the insert anyway (as a 500).
        if (request.Hero1Id == request.Hero2Id)
        {
            return (EnumBattleOutcome.SameHero, null);
        }

        // One query for both combatants rather than two round trips.
        var heroes = await db.Superheroes
            .AsNoTracking()
            .Where(h => h.Id == request.Hero1Id || h.Id == request.Hero2Id)
            .ToListAsync(cancellationToken);

        var hero1 = heroes.FirstOrDefault(h => h.Id == request.Hero1Id);
        var hero2 = heroes.FirstOrDefault(h => h.Id == request.Hero2Id);

        if (hero1 is null || hero2 is null)
        {
            return (EnumBattleOutcome.HeroNotFound, null);
        }

        var outcome = calculator.Simulate(hero1, hero2);

        var battle = new ModelBattle
        {
            Hero1Id = hero1.Id,
            Hero2Id = hero2.Id,
            WinnerId = outcome.WinnerId,
            Hero1Score = outcome.Hero1Score,
            Hero2Score = outcome.Hero2Score,
            BattleDate = DateTime.UtcNow,
        };

        db.Battles.Add(battle);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Battle {BattleId}: {Hero1} ({Score1}) vs {Hero2} ({Score2}) - winner {WinnerId}",
            battle.Id, hero1.Name, outcome.Hero1Score, hero2.Name, outcome.Hero2Score, outcome.WinnerId);

        var winner = outcome.WinnerId == hero1.Id ? hero1 : outcome.WinnerId == hero2.Id ? hero2 : null;
        var loser = outcome.WinnerId == hero1.Id ? hero2 : outcome.WinnerId == hero2.Id ? hero1 : null;

        return (EnumBattleOutcome.Success, new ModelBattleResult(
            BattleId: battle.Id,
            Hero1: ToCombatant(hero1, outcome.Hero1Score),
            Hero2: ToCombatant(hero2, outcome.Hero2Score),
            WinnerId: winner?.Id,
            WinnerName: winner?.Name,
            LoserId: loser?.Id,
            LoserName: loser?.Name,
            IsDraw: outcome.IsDraw,
            Summary: outcome.Summary,
            Breakdown: outcome.Breakdown,
            BattleDate: battle.BattleDate));
    }

    public async Task<ModelPagedResponse<ModelBattleListItem>> GetHistoryAsync(
        int page, int pageSize, int? superheroId, CancellationToken cancellationToken)
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = db.Battles.AsNoTracking();

        if (superheroId is { } heroId)
        {
            query = query.Where(b => b.Hero1Id == heroId || b.Hero2Id == heroId);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            // Id is the tie-breaker: several battles can share a BattleDate at this precision,
            // and without it paging over them is not deterministic.
            .OrderByDescending(b => b.BattleDate)
            .ThenByDescending(b => b.Id)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(b => new ModelBattleListItem(
                b.Id,
                b.Hero1Id, b.Hero1.Name, b.Hero1.ImageUrl, b.Hero1Score,
                b.Hero2Id, b.Hero2.Name, b.Hero2.ImageUrl, b.Hero2Score,
                b.WinnerId,
                b.Winner != null ? b.Winner.Name : null,
                b.BattleDate))
            .ToListAsync(cancellationToken);

        return new ModelPagedResponse<ModelBattleListItem>(items, safePage, safePageSize, totalCount);
    }

    public async Task<ModelBattleResult?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var battle = await db.Battles
            .AsNoTracking()
            .Include(b => b.Hero1)
            .Include(b => b.Hero2)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (battle is null)
        {
            return null;
        }

        // Re-derive the breakdown from the stored combatants. The scores themselves are read
        // from the record, NOT recalculated - re-running the simulation would produce different
        // numbers because of the variance roll, and a historical result must never change.
        var breakdown = calculator.Simulate(battle.Hero1, battle.Hero2).Breakdown;

        var winner = battle.WinnerId == battle.Hero1Id ? battle.Hero1
                   : battle.WinnerId == battle.Hero2Id ? battle.Hero2
                   : null;
        var loser = battle.WinnerId == battle.Hero1Id ? battle.Hero2
                  : battle.WinnerId == battle.Hero2Id ? battle.Hero1
                  : null;

        var isDraw = battle.WinnerId is null;
        var summary = isDraw
            ? $"{battle.Hero1.Name} and {battle.Hero2.Name} fought to a standstill at {battle.Hero1Score} apiece."
            : $"{winner!.Name} defeated {loser!.Name} " +
              $"{(battle.WinnerId == battle.Hero1Id ? battle.Hero1Score : battle.Hero2Score)}-" +
              $"{(battle.WinnerId == battle.Hero1Id ? battle.Hero2Score : battle.Hero1Score)}.";

        return new ModelBattleResult(
            battle.Id,
            ToCombatant(battle.Hero1, battle.Hero1Score),
            ToCombatant(battle.Hero2, battle.Hero2Score),
            winner?.Id, winner?.Name,
            loser?.Id, loser?.Name,
            isDraw,
            summary,
            breakdown,
            battle.BattleDate);
    }

    private static ModelBattleCombatant ToCombatant(ModelSuperhero hero, int score) =>
        new(hero.Id, hero.Name, hero.Universe, hero.Alignment, hero.ImageUrl, hero.PowerLevel, score);
}
