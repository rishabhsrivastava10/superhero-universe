using Microsoft.EntityFrameworkCore;
using WebAPISuperheroUniverse.DBContext.EntityFramework;
using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;
using WebAPISuperheroUniverse.Entities.Tables;

namespace WebAPISuperheroUniverse.API.Services;

public sealed class SuperheroService(AppDbContext db, ILogger<SuperheroService> logger) : ISuperheroService
{
    private const int MaxPageSize = 100;

    public async Task<ModelPagedResponse<ModelSuperheroListItem>> GetPagedAsync(ModelSuperheroQuery query, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        // Clamped so a caller cannot request pageSize=1000000 and pull the whole table.
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        // AsNoTracking: this is a read-only projection, so EF's change-tracking snapshots would be pure overhead.
        var filtered = ApplyFilters(db.Superheroes.AsNoTracking(), query);

        // Counted before paging, and on the same filtered query, so TotalCount reflects the filters.
        var totalCount = await filtered.CountAsync(cancellationToken);

        var items = await ApplySorting(filtered, query)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            // Projecting inside the query means SQL Server returns only these columns, and the
            // powers come back via a join rather than a follow-up query per hero (no N+1).
            .Select(h => new ModelSuperheroListItem(
                h.Id,
                h.Name,
                h.RealName,
                h.Universe,
                h.Alignment,
                h.PowerLevel,
                h.ImageUrl,
                h.SuperheroPowers.Select(sp => sp.Power.Name).ToList()))
            .ToListAsync(cancellationToken);

        return new ModelPagedResponse<ModelSuperheroListItem>(items, page, pageSize, totalCount);
    }

    public async Task<ModelSuperheroDetail?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        await db.Superheroes
            .AsNoTracking()
            .Where(h => h.Id == id)
            .Select(h => new ModelSuperheroDetail(
                h.Id, h.Name, h.RealName, h.Universe, h.Alignment, h.Description,
                h.PowerLevel, h.Intelligence, h.Strength, h.Speed, h.Durability, h.Combat,
                h.ImageUrl, h.CreatedAt, h.UpdatedAt,
                h.SuperheroPowers.Select(sp => sp.Power.Name).ToList(),
                h.SuperheroTeams.Select(st => st.Team.Name).ToList(),
                db.Battles.Count(b => b.Hero1Id == h.Id || b.Hero2Id == h.Id),
                db.Battles.Count(b => b.WinnerId == h.Id)))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<(EnumCrudOutcome, ModelSuperheroDetail?)> CreateAsync(ModelSuperheroCreateRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        if (await db.Superheroes.AnyAsync(h => h.Name == name, cancellationToken))
        {
            return (EnumCrudOutcome.DuplicateName, null);
        }

        var superhero = new ModelSuperhero
        {
            Name = name,
            RealName = request.RealName?.Trim(),
            Universe = request.Universe.Trim(),
            Alignment = request.Alignment.Trim(),
            Description = request.Description,
            PowerLevel = request.PowerLevel,
            Intelligence = request.Intelligence,
            Strength = request.Strength,
            Speed = request.Speed,
            Durability = request.Durability,
            Combat = request.Combat,
            ImageUrl = request.ImageUrl?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        db.Superheroes.Add(superhero);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Superhero {Name} created with id {Id}", superhero.Name, superhero.Id);

        return (EnumCrudOutcome.Success, await GetByIdAsync(superhero.Id, cancellationToken));
    }

    public async Task<(EnumCrudOutcome, ModelSuperheroDetail?)> UpdateAsync(int id, ModelSuperheroUpdateRequest request, CancellationToken cancellationToken)
    {
        // Tracked (no AsNoTracking) - this entity is being modified and saved back.
        var superhero = await db.Superheroes.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
        if (superhero is null)
        {
            return (EnumCrudOutcome.NotFound, null);
        }

        var name = request.Name.Trim();

        // Excludes itself, so saving a hero without renaming it isn't treated as a duplicate.
        if (await db.Superheroes.AnyAsync(h => h.Name == name && h.Id != id, cancellationToken))
        {
            return (EnumCrudOutcome.DuplicateName, null);
        }

        superhero.Name = name;
        superhero.RealName = request.RealName?.Trim();
        superhero.Universe = request.Universe.Trim();
        superhero.Alignment = request.Alignment.Trim();
        superhero.Description = request.Description;
        superhero.PowerLevel = request.PowerLevel;
        superhero.Intelligence = request.Intelligence;
        superhero.Strength = request.Strength;
        superhero.Speed = request.Speed;
        superhero.Durability = request.Durability;
        superhero.Combat = request.Combat;
        superhero.ImageUrl = request.ImageUrl?.Trim();
        superhero.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Superhero {Id} updated", id);

        return (EnumCrudOutcome.Success, await GetByIdAsync(id, cancellationToken));
    }

    public async Task<EnumCrudOutcome> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var superhero = await db.Superheroes.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
        if (superhero is null)
        {
            return EnumCrudOutcome.NotFound;
        }

        // xtBattles has three FKs back to xtSuperheroes, all ON DELETE NO ACTION (SQL Server
        // refuses multiple cascade paths to the same table). Deleting a hero with battle history
        // would therefore fail at the database with an FK violation. Checking up front turns that
        // into a clear 409 instead of a 500, and preserves battle history as a permanent record.
        var hasBattles = await db.Battles.AnyAsync(
            b => b.Hero1Id == id || b.Hero2Id == id || b.WinnerId == id, cancellationToken);

        if (hasBattles)
        {
            logger.LogInformation("Delete of superhero {Id} blocked - battle history exists", id);
            return EnumCrudOutcome.InUse;
        }

        // Power/team/mission links cascade automatically, so they need no explicit cleanup.
        db.Superheroes.Remove(superhero);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Superhero {Id} deleted", id);
        return EnumCrudOutcome.Success;
    }

    private static IQueryable<ModelSuperhero> ApplyFilters(IQueryable<ModelSuperhero> source, ModelSuperheroQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            // Translated to SQL LIKE '%term%'. Leading wildcards can't use an index seek, which is
            // acceptable at this table size; full-text search would be the answer if it grew large.
            source = source.Where(h => EF.Functions.Like(h.Name, $"%{search}%")
                                       || (h.RealName != null && EF.Functions.Like(h.RealName, $"%{search}%")));
        }

        if (!string.IsNullOrWhiteSpace(query.Universe))
        {
            var universe = query.Universe.Trim();
            source = source.Where(h => h.Universe == universe);
        }

        if (!string.IsNullOrWhiteSpace(query.Alignment))
        {
            var alignment = query.Alignment.Trim();
            source = source.Where(h => h.Alignment == alignment);
        }

        if (query.MinPowerLevel is { } min)
        {
            source = source.Where(h => h.PowerLevel >= min);
        }

        if (query.MaxPowerLevel is { } max)
        {
            source = source.Where(h => h.PowerLevel <= max);
        }

        return source;
    }

    private static IQueryable<ModelSuperhero> ApplySorting(IQueryable<ModelSuperhero> source, ModelSuperheroQuery query)
    {
        var descending = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
        var sortBy = query.SortBy?.Trim().ToLowerInvariant();

        // Switch over an allow-list rather than building an expression from the raw string -
        // unknown values fall through to the default instead of reaching the database.
        IOrderedQueryable<ModelSuperhero> ordered = sortBy switch
        {
            "name" => descending ? source.OrderByDescending(h => h.Name) : source.OrderBy(h => h.Name),
            "powerlevel" => descending ? source.OrderByDescending(h => h.PowerLevel) : source.OrderBy(h => h.PowerLevel),
            "intelligence" => descending ? source.OrderByDescending(h => h.Intelligence) : source.OrderBy(h => h.Intelligence),
            "strength" => descending ? source.OrderByDescending(h => h.Strength) : source.OrderBy(h => h.Strength),
            "speed" => descending ? source.OrderByDescending(h => h.Speed) : source.OrderBy(h => h.Speed),
            "durability" => descending ? source.OrderByDescending(h => h.Durability) : source.OrderBy(h => h.Durability),
            "combat" => descending ? source.OrderByDescending(h => h.Combat) : source.OrderBy(h => h.Combat),
            "createdat" => descending ? source.OrderByDescending(h => h.CreatedAt) : source.OrderBy(h => h.CreatedAt),
            _ => source.OrderBy(h => h.Name)
        };

        // Paging over a non-unique sort key (many heroes can share a PowerLevel) has no guaranteed
        // row order, so the same hero can appear on two pages. Id is the stable tie-breaker.
        return ordered.ThenBy(h => h.Id);
    }
}
