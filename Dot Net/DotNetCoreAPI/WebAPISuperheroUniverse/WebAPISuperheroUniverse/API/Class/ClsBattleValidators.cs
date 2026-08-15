using FluentValidation;
using WebAPISuperheroUniverse.Entities.DTOs;

namespace WebAPISuperheroUniverse.API.Class;

public sealed class ClsSimulateBattleRequestValidator : AbstractValidator<ModelSimulateBattleRequest>
{
    public ClsSimulateBattleRequestValidator()
    {
        RuleFor(x => x.Hero1Id).GreaterThan(0).WithMessage("Select the first superhero.");
        RuleFor(x => x.Hero2Id).GreaterThan(0).WithMessage("Select the second superhero.");

        // Also enforced in the service (and by CK_xtBattles_DistinctHeroes in the database).
        // Catching it here gives a field-level message instead of a generic error.
        RuleFor(x => x.Hero2Id)
            .NotEqual(x => x.Hero1Id)
            .WithMessage("A superhero cannot battle themselves. Choose two different heroes.");
    }
}
