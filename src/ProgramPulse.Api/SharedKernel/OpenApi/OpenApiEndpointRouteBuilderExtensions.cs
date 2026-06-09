using ProgramPulse.Api.Infrastructure;
using ProgramPulse.Api.SharedKernel.Versioning;
using Scalar.AspNetCore;

namespace ProgramPulse.Api.SharedKernel.OpenApi;

public static class OpenApiEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApiDocumentation(
        this IEndpointRouteBuilder app,
        IWebHostEnvironment env)
    {
        if (!env.IsDevelopmentOrLocal())
            return app;

        // Serves /openapi/{documentName}.json for every registered version document.
        app.MapOpenApi();

        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle(ApiDocsConstants.Title)
                .WithClassicLayout()
                .WithOpenApiRoutePattern("/openapi/{documentName}.json")
                .AddPreferredSecuritySchemes(ApiDocsConstants.BearerScheme)
                .AddHttpAuthentication(ApiDocsConstants.BearerScheme, _ => { });

            foreach (var version in ApiVersions.All)
                options.AddDocument($"v{version.MajorVersion}");
        });

        return app;
    }
}
