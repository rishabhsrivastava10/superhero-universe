using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebAPISuperheroUniverse.Entities.Models;

namespace WebAPISuperheroUniverse.API.Class;

/// <summary>
/// Runs any registered FluentValidation validator against each action argument before the action
/// executes, and short-circuits with the standard error shape if validation fails.
///
/// Registered globally so no controller has to remember to validate - forgetting is the usual way
/// unvalidated input reaches a service.
/// </summary>
public sealed class ClsValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var failures = new Dictionary<string, List<string>>();

        foreach (var argument in context.ActionArguments.Values.Where(a => a is not null))
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(argument!.GetType());
            if (serviceProvider.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

            foreach (var error in result.Errors)
            {
                if (!failures.TryGetValue(error.PropertyName, out var messages))
                {
                    messages = [];
                    failures[error.PropertyName] = messages;
                }

                messages.Add(error.ErrorMessage);
            }
        }

        if (failures.Count > 0)
        {
            context.Result = new BadRequestObjectResult(new ModelErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "One or more validation errors occurred.",
                Timestamp = DateTime.UtcNow,
                Errors = failures.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray())
            });

            return;
        }

        await next();
    }
}
