using FluentValidation;

namespace ProgramPulse.Api.SharedKernel.Validation;

public static class ValidationServiceCollectionExtensions
{
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);
        return services;
    }
}
