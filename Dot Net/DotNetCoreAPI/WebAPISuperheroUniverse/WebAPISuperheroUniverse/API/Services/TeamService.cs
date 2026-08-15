using Microsoft.EntityFrameworkCore;
using WebAPISuperheroUniverse.DBContext.EntityFramework;
using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;
using WebAPISuperheroUniverse.Entities.Tables;

namespace WebAPISuperheroUniverse.API.Services;

public sealed class TeamService(AppDbContext db, ILogger<TeamService> logger) : ITeamService
{
    public async Task<IReadOnlyList<ModelTeamListItem>> GetAllAsync(string? universe, CancellationToken cancellationToken)
    {
        var query = db.Teams.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(universe))
        {
            var filter = universe.Trim();
            query = query.Where(t => t.Universe == filter);
        }

        return await query
            .OrderBy(t => t.Name)
            .Select(t => new ModelTeamListItem(
                t.Id, t.Name, t.Universe, t.Description, t.FoundedDate, t.SuperheroTeams.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<ModelTeamDetail?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var team = await db.Teams
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Universe,
                t.Description,
                t.FoundedDate,
                Members = t.SuperheroTeams
                    .OrderByDescending(st => st.Superhero.PowerLevel)
                    .Select(st => new ModelTeamMember(
                        st.SuperheroId,
                        st.Superhero.Name,
                        st.Superhero.RealName,
                        st.Superhero.Alignment,
                        st.Superhero.PowerLevel,
                        st.Superhero.ImageUrl,
                        st.JoinedDate))
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (team is null)
        {
            return null;
        }

        // Computed in memory: the member list is already materialised and is small (a team has
        // tens of members at most), so a second round trip to aggregate in SQL would cost more
        // than it saves.
        var members = team.Members;
        var statistics = new ModelTeamStatistics(
            MemberCount: members.Count,
            TotalPowerLevel: members.Sum(m => m.PowerLevel),
            AveragePowerLevel: members.Count == 0 ? 0 : Math.Round(members.Average(m => m.PowerLevel), 1),
            StrongestMemberPowerLevel: members.Count == 0 ? 0 : members.Max(m => m.PowerLevel),
            StrongestMemberName: members.MaxBy(m => m.PowerLevel)?.Name,
            HeroCount: members.Count(m => m.Alignment == "Hero"),
            VillainCount: members.Count(m => m.Alignment == "Villain"),
            AntiHeroCount: members.Count(m => m.Alignment == "Anti-Hero"));

        return new ModelTeamDetail(
            team.Id, team.Name, team.Universe, team.Description, team.FoundedDate, members, statistics);
    }

    public async Task<(EnumCrudOutcome, ModelTeamDetail?)> CreateAsync(ModelTeamRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        if (await db.Teams.AnyAsync(t => t.Name == name, cancellationToken))
        {
            return (EnumCrudOutcome.DuplicateName, null);
        }

        var team = new ModelTeam
        {
            Name = name,
            Universe = request.Universe.Trim(),
            Description = request.Description?.Trim(),
            FoundedDate = request.FoundedDate,
        };

        db.Teams.Add(team);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Team {Name} created with id {Id}", team.Name, team.Id);

        return (EnumCrudOutcome.Success, await GetByIdAsync(team.Id, cancellationToken));
    }

    public async Task<(EnumCrudOutcome, ModelTeamDetail?)> UpdateAsync(int id, ModelTeamRequest request, CancellationToken cancellationToken)
    {
        var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (team is null)
        {
            return (EnumCrudOutcome.NotFound, null);
        }

        var name = request.Name.Trim();

        if (await db.Teams.AnyAsync(t => t.Name == name && t.Id != id, cancellationToken))
        {
            return (EnumCrudOutcome.DuplicateName, null);
        }

        team.Name = name;
        team.Universe = request.Universe.Trim();
        team.Description = request.Description?.Trim();
        team.FoundedDate = request.FoundedDate;

        await db.SaveChangesAsync(cancellationToken);

        return (EnumCrudOutcome.Success, await GetByIdAsync(id, cancellationToken));
    }

    public async Task<EnumCrudOutcome> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (team is null)
        {
            return EnumCrudOutcome.NotFound;
        }

        // Unlike powers, deleting a team is allowed even with members: the membership rows are
        // the only thing lost, and no hero record is harmed. The cascade handles them.
        db.Teams.Remove(team);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Team {Id} deleted", id);
        return EnumCrudOutcome.Success;
    }

    public async Task<EnumCrudOutcome> AddMemberAsync(int teamId, int superheroId, CancellationToken cancellationToken)
    {
        var teamExists = await db.Teams.AnyAsync(t => t.Id == teamId, cancellationToken);
        var heroExists = await db.Superheroes.AnyAsync(h => h.Id == superheroId, cancellationToken);

        if (!teamExists || !heroExists)
        {
            return EnumCrudOutcome.NotFound;
        }

        var alreadyMember = await db.SuperheroTeams
            .AnyAsync(st => st.TeamId == teamId && st.SuperheroId == superheroId, cancellationToken);

        if (alreadyMember)
        {
            // Checked explicitly so the caller gets a 409 rather than a primary-key violation.
            return EnumCrudOutcome.DuplicateName;
        }

        db.SuperheroTeams.Add(new ModelSuperheroTeam
        {
            TeamId = teamId,
            SuperheroId = superheroId,
            JoinedDate = DateOnly.FromDateTime(DateTime.UtcNow),
        });

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Superhero {SuperheroId} joined team {TeamId}", superheroId, teamId);
        return EnumCrudOutcome.Success;
    }

    public async Task<EnumCrudOutcome> RemoveMemberAsync(int teamId, int superheroId, CancellationToken cancellationToken)
    {
        var membership = await db.SuperheroTeams
            .FirstOrDefaultAsync(st => st.TeamId == teamId && st.SuperheroId == superheroId, cancellationToken);

        if (membership is null)
        {
            return EnumCrudOutcome.NotFound;
        }

        db.SuperheroTeams.Remove(membership);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Superhero {SuperheroId} left team {TeamId}", superheroId, teamId);
        return EnumCrudOutcome.Success;
    }
}
