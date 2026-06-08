using FluentValidation;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.SharedKernel.Validation;

public sealed class ValidationFilter<TRequest>(IValidator<TRequest> validator) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (request is null)
            return await next(context);

        var result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
        if (!result.IsValid)
            return Result.Failure(result.ToValidationError()).ToHttpResult();

        return await next(context);
    }
}

public static class ValidationFilterExtensions
{
    // Usage on a route: app.MapPost(...).WithValidation<CreateXRequest>();
    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder) =>
        builder.AddEndpointFilter<ValidationFilter<TRequest>>();
}
