using FluentValidation;
using WebAPISuperheroUniverse.Entities.DTOs;

namespace WebAPISuperheroUniverse.API.Class;

public sealed class ClsRegisterRequestValidator : AbstractValidator<ModelRegisterRequest>
{
    public ClsRegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters.")
            .Matches("^[a-zA-Z0-9._-]+$").WithMessage("Username may only contain letters, numbers, dot, underscore or hyphen.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
    }
}

public sealed class ClsLoginRequestValidator : AbstractValidator<ModelLoginRequest>
{
    public ClsLoginRequestValidator()
    {
        // Deliberately only checks presence. Applying the registration password rules here would
        // reject a login before checking credentials, leaking which passwords could not exist.
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}

public sealed class ClsRefreshRequestValidator : AbstractValidator<ModelRefreshRequest>
{
    public ClsRefreshRequestValidator()
    {
        RuleFor(x => x.AccessToken).NotEmpty().WithMessage("Access token is required.");
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token is required.");
    }
}
