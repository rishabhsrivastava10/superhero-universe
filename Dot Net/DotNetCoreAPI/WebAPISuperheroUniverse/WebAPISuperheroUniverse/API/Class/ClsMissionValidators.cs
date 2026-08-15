using FluentValidation;
using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;

namespace WebAPISuperheroUniverse.API.Class;

public sealed class ClsMissionRequestValidator : AbstractValidator<ModelMissionRequest>
{
    private static readonly string[] ValidDifficulties =
        [.. Enum.GetNames<EnumMissionDifficulty>()];

    public ClsMissionRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Mission title is required.")
            .MaximumLength(200).WithMessage("Mission title must not exceed 200 characters.");

        RuleFor(x => x.Location)
            .MaximumLength(150).WithMessage("Location must not exceed 150 characters.");

        // Mirrors CK_xtMissions_Difficulty.
        RuleFor(x => x.Difficulty)
            .NotEmpty().WithMessage("Difficulty is required.")
            .Must(d => ValidDifficulties.Contains(d))
            .WithMessage($"Difficulty must be one of: {string.Join(", ", ValidDifficulties)}.");

        // Mirrors CK_xtMissions_RequiredHeroCount. The upper bound is a sanity limit rather than
        // a database rule - a mission needing 500 heroes is a typo, not a design.
        RuleFor(x => x.RequiredHeroCount)
            .InclusiveBetween(1, 20)
            .WithMessage("Required hero count must be between 1 and 20.");
    }
}

public sealed class ClsStartMissionRequestValidator : AbstractValidator<ModelStartMissionRequest>
{
    public ClsStartMissionRequestValidator()
    {
        RuleFor(x => x.SuperheroIds)
            .NotNull().WithMessage("Select at least one superhero.")
            .Must(ids => ids is { Count: > 0 }).WithMessage("Select at least one superhero.");

        RuleForEach(x => x.SuperheroIds)
            .GreaterThan(0).WithMessage("Superhero ids must be positive.");
    }
}
