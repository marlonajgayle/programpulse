using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProgramPulse.Api.Infrastructure.RateLimiting;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.SharedKernel;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        var descriptors = typeof(Program).Assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && typeof(IEndpoint).IsAssignableFrom(t))
            .Select(t => ServiceDescriptor.Transient(typeof(IEndpoint), t));

        services.TryAddEnumerable(descriptors);
        return services;
    }

    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        ApiVersionSet versionSet = app.NewApiVersionSet()
            .HasApiVersion(ApiVersions.V1)
            .ReportApiVersions()
            .Build();

        RouteGroupBuilder versioned = app
            .MapGroup("api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet)
            .RequireRateLimiting(RateLimitPolicies.IpFixedWindow);

        foreach (var endpoint in app.Services.GetRequiredService<IEnumerable<IEndpoint>>())
            endpoint.MapEndpoint(versioned);

        return app;
    }
}
