using Microsoft.EntityFrameworkCore;
using WebAPISuperheroUniverse.API.Class;
using WebAPISuperheroUniverse.DBContext.EntityFramework;
using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;
using WebAPISuperheroUniverse.Entities.Tables;

namespace WebAPISuperheroUniverse.API.Services;

public sealed class MissionService(
    AppDbContext db,
    ClsMissionCalculator calculator,
    ILogger<MissionService> logger) : IMissionService
{
    public async Task<IReadOnlyList<ModelMissionListItem>> GetAllAsync(string? status, CancellationToken cancellationToken)
    {
        var query = db.Missions.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var filter = status.Trim();
            query = query.Where(m => m.Status == filter);
        }

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id)
            .Select(m => new ModelMissionListItem(
                m.Id, m.Title, m.Description, m.Location, m.Difficulty,
                m.RequiredHeroCount, m.Status, m.MissionHeroes.Count, m.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<ModelMissionDetail?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        await db.Missions
            .AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new ModelMissionDetail(
                m.Id, m.Title, m.Description, m.Location, m.Difficulty,
                m.RequiredHeroCount, m.Status, m.CreatedAt,
                m.MissionHeroes
                    .OrderByDescending(mh => mh.Superhero.PowerLevel)
                    .Select(mh => new ModelMissionHeroSummary(
                        mh.SuperheroId, mh.Superhero.Name, mh.Superhero.Alignment,
                        mh.Superhero.PowerLevel, mh.Superhero.ImageUrl))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<(EnumCrudOutcome, ModelMissionDetail?)> CreateAsync(ModelMissionRequest request, CancellationToken cancellationToken)
    {
        var title = request.Title.Trim();

        if (await db.Missions.AnyAsync(m => m.Title == title, cancellationToken))
        {
            return (EnumCrudOutcome.DuplicateName, null);
        }

        var mission = new ModelMission
        {
            Title = title,
            Description = request.Description?.Trim(),
            Location = request.Location?.Trim(),
            Difficulty = request.Difficulty.Trim(),
            RequiredHeroCount = request.RequiredHeroCount,
            Status = nameof(EnumMissionStatus.Pending),
            CreatedAt = DateTime.UtcNow,
        };

        db.Missions.Add(mission);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Mission {Title} created with id {Id}", mission.Title, mission.Id);

        return (EnumCrudOutcome.Success, await GetByIdAsync(mission.Id, cancellationToken));
    }

    public async Task<(EnumCrudOutcome, ModelMissionDetail?)> UpdateAsync(int id, ModelMissionRequest request, CancellationToken cancellationToken)
    {
        var mission = await db.Missions.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (mission is null)
        {
            return (EnumCrudOutcome.NotFound, null);
        }

        var title = request.Title.Trim();

        if (await db.Missions.AnyAsync(m => m.Title == title && m.Id != id, cancellationToken))
        {
            return (EnumCrudOutcome.DuplicateName, null);
        }

        mission.Title = title;
        mission.Description = request.Description?.Trim();
        mission.Location = request.Location?.Trim();
        mission.Difficulty = request.Difficulty.Trim();
        mission.RequiredHeroCount = request.RequiredHeroCount;

        await db.SaveChangesAsync(cancellationToken);

        return (EnumCrudOutcome.Success, await GetByIdAsync(id, cancellationToken));
    }

    public async Task<EnumCrudOutcome> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var mission = await db.Missions.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (mission is null)
        {
            return EnumCrudOutcome.NotFound;
        }

        // xtMissionHeroes cascades, and no hero record is harmed - only the assignment rows.
        db.Missions.Remove(mission);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Mission {Id} deleted", id);
        return EnumCrudOutcome.Success;
    }

    public async Task<(EnumMissionOutcome, ModelMissionResult?)> StartAsync(
        int id, ModelStartMissionRequest request, CancellationToken cancellationToken)
    {
        var mission = await db.Missions
            .Include(m => m.MissionHeroes)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (mission is null)
        {
            return (EnumMissionOutcome.MissionNotFound, null);
        }

        // A resolved mission is history. Re-running it would silently overwrite the record,
        // so an explicit reset is required first.
        if (mission.Status is nameof(EnumMissionStatus.Success) or nameof(EnumMissionStatus.Failed))
        {
            return (EnumMissionOutcome.AlreadyResolved, null);
        }

        // Distinct guards the composite key on xtMissionHeroes against a repeated id.
        var heroIds = request.SuperheroIds.Distinct().ToList();
        if (heroIds.Count == 0)
        {
            return (EnumMissionOutcome.NoHeroes, null);
        }

        var squad = await db.Superheroes
            .AsNoTracking()
            .Where(h => heroIds.Contains(h.Id))
            .ToListAsync(cancellationToken);

        // Silently dropping an unknown id would let the caller think their full squad deployed.
        if (squad.Count != heroIds.Count)
        {
            return (EnumMissionOutcome.HeroNotFound, null);
        }

        var outcome = calculator.Resolve(mission, squad);

        // Record who actually went, replacing any earlier assignment.
        mission.MissionHeroes.Clear();
        foreach (var heroId in heroIds)
        {
            mission.MissionHeroes.Add(new ModelMissionHero { MissionId = mission.Id, SuperheroId = heroId });
        }

        mission.Status = outcome.Succeeded
            ? nameof(EnumMissionStatus.Success)
            : nameof(EnumMissionStatus.Failed);

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Mission {MissionId} attempted by {SquadSize} hero(es): chance {Chance}%, rolled {Roll} -> {Status}",
            mission.Id, squad.Count, outcome.SuccessChancePercent, outcome.RollPercent, mission.Status);

        var squadSummaries = squad
            .OrderByDescending(h => h.PowerLevel)
            .Select(h => new ModelMissionHeroSummary(h.Id, h.Name, h.Alignment, h.PowerLevel, h.ImageUrl))
            .ToList();

        return (EnumMissionOutcome.Success, new ModelMissionResult(
            mission.Id, mission.Title, mission.Difficulty, mission.Status,
            outcome.Succeeded, outcome.SuccessChancePercent, outcome.RollPercent,
            outcome.Summary, squadSummaries, outcome.Factors));
    }

    public async Task<EnumCrudOutcome> ResetAsync(int id, CancellationToken cancellationToken)
    {
        var mission = await db.Missions
            .Include(m => m.MissionHeroes)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (mission is null)
        {
            return EnumCrudOutcome.NotFound;
        }

        mission.Status = nameof(EnumMissionStatus.Pending);
        mission.MissionHeroes.Clear();
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Mission {Id} reset to Pending", id);
        return EnumCrudOutcome.Success;
    }
}
