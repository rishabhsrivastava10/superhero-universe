using FluentValidation;
using WebAPISuperheroUniverse.Entities.DTOs;

namespace WebAPISuperheroUniverse.API.Class;

public sealed class ClsPowerRequestValidator : AbstractValidator<ModelPowerRequest>
{
    public ClsPowerRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Power name is required.")
            .MaximumLength(100).WithMessage("Power name must not exceed 100 characters.");
    }
}

public sealed class ClsAssignPowersRequestValidator : AbstractValidator<ModelAssignPowersRequest>
{
    public ClsAssignPowersRequestValidator()
    {
        // An empty list is legitimate - it means "this hero has no powers" (e.g. Batman).
        RuleFor(x => x.PowerIds)
            .NotNull().WithMessage("PowerIds is required (send an empty array to clear all powers).");

        RuleForEach(x => x.PowerIds)
            .GreaterThan(0).WithMessage("Power ids must be positive.");
    }
}

public sealed class ClsTeamRequestValidator : AbstractValidator<ModelTeamRequest>
{
    public ClsTeamRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Team name is required.")
            .MaximumLength(100).WithMessage("Team name must not exceed 100 characters.");

        RuleFor(x => x.Universe)
            .NotEmpty().WithMessage("Universe is required.")
            .MaximumLength(50).WithMessage("Universe must not exceed 50 characters.");

        RuleFor(x => x.FoundedDate)
            .Must(date => date is null || date <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Founded date cannot be in the future.");
    }
}

public sealed class ClsAddTeamMemberRequestValidator : AbstractValidator<ModelAddTeamMemberRequest>
{
    public ClsAddTeamMemberRequestValidator() =>
        RuleFor(x => x.SuperheroId).GreaterThan(0).WithMessage("A valid superhero id is required.");
}
