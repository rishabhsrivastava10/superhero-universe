using FluentValidation;
using WebAPISuperheroUniverse.Entities.DTOs;
using WebAPISuperheroUniverse.Entities.Enums;

namespace WebAPISuperheroUniverse.API.Class;

/// <summary>
/// Rules shared by create and update. Every stat rule mirrors a CHECK constraint in the database -
/// the DB is the last line of defence, but a 400 with a clear message beats a 500 from a
/// constraint violation.
/// </summary>
internal static class ClsSuperheroRules
{
    private static readonly string[] ValidAlignments =
        [.. Enum.GetValues<EnumAlignment>().Select(a => a.ToDbValue())];

    public static void ApplyTo<T>(AbstractValidator<T> validator,
        Func<T, string> name,
        Func<T, string?> realName,
        Func<T, string> universe,
        Func<T, string> alignment,
        Func<T, string?> imageUrl,
        Func<T, int> powerLevel,
        Func<T, int> intelligence,
        Func<T, int> strength,
        Func<T, int> speed,
        Func<T, int> durability,
        Func<T, int> combat)
    {
        validator.RuleFor(x => name(x))
            .NotEmpty().WithName("Name").WithMessage("Hero name is required.")
            .MaximumLength(100).WithName("Name").WithMessage("Hero name must not exceed 100 characters.");

        validator.RuleFor(x => realName(x))
            .MaximumLength(150).WithName("RealName").WithMessage("Real name must not exceed 150 characters.");

        validator.RuleFor(x => universe(x))
            .NotEmpty().WithName("Universe").WithMessage("Universe is required.")
            .MaximumLength(50).WithName("Universe").WithMessage("Universe must not exceed 50 characters.");

        validator.RuleFor(x => alignment(x))
            .NotEmpty().WithName("Alignment").WithMessage("Alignment is required.")
            .Must(a => ValidAlignments.Contains(a))
            .WithName("Alignment")
            .WithMessage($"Alignment must be one of: {string.Join(", ", ValidAlignments)}.");

        validator.RuleFor(x => imageUrl(x))
            .MaximumLength(500).WithName("ImageUrl").WithMessage("Image URL must not exceed 500 characters.");

        AddStatRule(validator, powerLevel, "PowerLevel");
        AddStatRule(validator, intelligence, "Intelligence");
        AddStatRule(validator, strength, "Strength");
        AddStatRule(validator, speed, "Speed");
        AddStatRule(validator, durability, "Durability");
        AddStatRule(validator, combat, "Combat");
    }

    private static void AddStatRule<T>(AbstractValidator<T> validator, Func<T, int> selector, string displayName) =>
        validator.RuleFor(x => selector(x))
            .InclusiveBetween(0, 100)
            .WithName(displayName)
            .WithMessage($"{displayName} must be between 0 and 100.");
}

public sealed class ClsSuperheroCreateRequestValidator : AbstractValidator<ModelSuperheroCreateRequest>
{
    public ClsSuperheroCreateRequestValidator() =>
        ClsSuperheroRules.ApplyTo(this,
            x => x.Name, x => x.RealName, x => x.Universe, x => x.Alignment, x => x.ImageUrl,
            x => x.PowerLevel, x => x.Intelligence, x => x.Strength, x => x.Speed, x => x.Durability, x => x.Combat);
}

public sealed class ClsSuperheroUpdateRequestValidator : AbstractValidator<ModelSuperheroUpdateRequest>
{
    public ClsSuperheroUpdateRequestValidator() =>
        ClsSuperheroRules.ApplyTo(this,
            x => x.Name, x => x.RealName, x => x.Universe, x => x.Alignment, x => x.ImageUrl,
            x => x.PowerLevel, x => x.Intelligence, x => x.Strength, x => x.Speed, x => x.Durability, x => x.Combat);
}

public sealed class ClsSuperheroQueryValidator : AbstractValidator<ModelSuperheroQuery>
{
    public ClsSuperheroQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).WithMessage("Page must be 1 or greater.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
        RuleFor(x => x.MinPowerLevel).InclusiveBetween(0, 100).When(x => x.MinPowerLevel.HasValue)
            .WithMessage("MinPowerLevel must be between 0 and 100.");
        RuleFor(x => x.MaxPowerLevel).InclusiveBetween(0, 100).When(x => x.MaxPowerLevel.HasValue)
            .WithMessage("MaxPowerLevel must be between 0 and 100.");
        RuleFor(x => x)
            .Must(q => !q.MinPowerLevel.HasValue || !q.MaxPowerLevel.HasValue || q.MinPowerLevel <= q.MaxPowerLevel)
            .WithName("MinPowerLevel")
            .WithMessage("MinPowerLevel cannot be greater than MaxPowerLevel.");
        RuleFor(x => x.SortBy)
            .Must(s => ModelSuperheroQuery.AllowedSortFields.Contains(s!.Trim().ToLowerInvariant()))
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy))
            .WithMessage($"SortBy must be one of: {string.Join(", ", ModelSuperheroQuery.AllowedSortFields)}.");
        RuleFor(x => x.SortDir)
            .Must(d => d!.Equals("asc", StringComparison.OrdinalIgnoreCase) || d.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .When(x => !string.IsNullOrWhiteSpace(x.SortDir))
            .WithMessage("SortDir must be either 'asc' or 'desc'.");
    }
}
