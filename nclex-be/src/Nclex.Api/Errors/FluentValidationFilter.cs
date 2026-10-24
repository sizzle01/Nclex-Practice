using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Nclex.Api.Errors;

/// <summary>
/// Runs the FluentValidation validator for each action argument that has one (for example
/// SignupRequestValidator for SignupRequest) and stops with 400 VALIDATION_FAILED on any error.
/// Controllers and handlers can then assume their input is valid.
/// </summary>
public sealed class FluentValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new List<FieldError>();
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var result = await validator.ValidateAsync(
                new ValidationContext<object>(argument),
                context.HttpContext.RequestAborted);
            errors.AddRange(result.Errors.Select(e =>
                new FieldError(ValidationProblems.ToFieldName(e.PropertyName), e.ErrorCode)));
        }

        if (errors.Count > 0)
        {
            context.Result = ValidationProblems.Create(errors);
            return;
        }

        await next();
    }
}
