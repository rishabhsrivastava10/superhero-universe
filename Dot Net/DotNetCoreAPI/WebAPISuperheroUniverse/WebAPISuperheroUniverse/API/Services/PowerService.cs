using Microsoft.EntityFrameworkCore;
using WebAPISuperheroUniverse.DBContext.EntityFramework;
using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;
using WebAPISuperheroUniverse.Entities.Tables;

namespace WebAPISuperheroUniverse.API.Services;

public sealed class PowerService(AppDbContext db, ILogger<PowerService> logger) : IPowerService
{
    public async Task<IReadOnlyList<ModelPowerListItem>> GetAllAsync(CancellationToken cancellationToken) =>
        await db.Powers
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new ModelPowerListItem(p.Id, p.Name, p.Description, p.SuperheroPowers.Count))
            .ToListAsync(cancellationToken);

    public async Task<ModelPowerListItem?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        await db.Powers
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ModelPowerListItem(p.Id, p.Name, p.Description, p.SuperheroPowers.Count))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<(EnumCrudOutcome, ModelPowerListItem?)> CreateAsync(ModelPowerRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        if (await db.Powers.AnyAsync(p => p.Name == name, cancellationToken))
        {
            return (EnumCrudOutcome.DuplicateName, null);
        }

        var power = new ModelPower { Name = name, Description = request.Description?.Trim() };
        db.Powers.Add(power);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Power {Name} created with id {Id}", power.Name, power.Id);

        return (EnumCrudOutcome.Success, new ModelPowerListItem(power.Id, power.Name, power.Description, 0));
    }

    public async Task<(EnumCrudOutcome, ModelPowerListItem?)> UpdateAsync(int id, ModelPowerRequest request, CancellationToken cancellationToken)
    {
        var power = await db.Powers.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (power is null)
        {
            return (EnumCrudOutcome.NotFound, null);
        }

        var name = request.Name.Trim();

        if (await db.Powers.AnyAsync(p => p.Name == name && p.Id != id, cancellationToken))
        {
            return (EnumCrudOutcome.DuplicateName, null);
        }

        power.Name = name;
        power.Description = request.Description?.Trim();
        await db.SaveChangesAsync(cancellationToken);

        return (EnumCrudOutcome.Success, await GetByIdAsync(id, cancellationToken));
    }

    public async Task<EnumCrudOutcome> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var power = await db.Powers.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (power is null)
        {
            return EnumCrudOutcome.NotFound;
        }

        // xtSuperheroPowers cascades, so this would silently strip the power from every hero
        // that has it. Blocking instead makes the consequence explicit - the admin must
        // unassign it first, which is a deliberate act rather than a surprise side effect.
        var inUse = await db.SuperheroPowers.AnyAsync(sp => sp.PowerId == id, cancellationToken);
        if (inUse)
        {
            return EnumCrudOutcome.InUse;
        }

        db.Powers.Remove(power);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Power {Id} deleted", id);
        return EnumCrudOutcome.Success;
    }

    public async Task<(EnumCrudOutcome, IReadOnlyList<string>?)> AssignToSuperheroAsync(
        int superheroId, ModelAssignPowersRequest request, CancellationToken cancellationToken)
    {
        var superhero = await db.Superheroes
            .Include(h => h.SuperheroPowers)
            .FirstOrDefaultAsync(h => h.Id == superheroId, cancellationToken);

        if (superhero is null)
        {
            return (EnumCrudOutcome.NotFound, null);
        }

        // Distinct guards against the same id being sent twice, which would violate the
        // composite primary key on xtSuperheroPowers.
        var requestedIds = request.PowerIds.Distinct().ToList();

        var validIds = await db.Powers
            .Where(p => requestedIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        // A power id that doesn't exist is a client mistake worth reporting, not something
        // to silently drop - otherwise the caller thinks the assignment fully succeeded.
        if (validIds.Count != requestedIds.Count)
        {
            return (EnumCrudOutcome.NotFound, null);
        }

        // Replace-the-whole-set semantics: simpler for the client than separate add/remove
        // calls, and it matches how the UI's multi-select actually behaves.
        superhero.SuperheroPowers.Clear();
        foreach (var powerId in validIds)
        {
            superhero.SuperheroPowers.Add(new ModelSuperheroPower { SuperheroId = superheroId, PowerId = powerId });
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Superhero {Id} now has {Count} power(s)", superheroId, validIds.Count);

        var names = await db.SuperheroPowers
            .Where(sp => sp.SuperheroId == superheroId)
            .OrderBy(sp => sp.Power.Name)
            .Select(sp => sp.Power.Name)
            .ToListAsync(cancellationToken);

        return (EnumCrudOutcome.Success, names);
    }
}
