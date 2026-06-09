using Asp.Versioning;

namespace ProgramPulse.Api.SharedKernel.Versioning;

public static class ApiVersioningServiceCollectionExtensions
{
    public static IServiceCollection AddApiVersioningWithOpenApi(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = ApiVersions.V1;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";          // -> "v1"
                options.SubstituteApiVersionInUrl = true;  // /api/v{version:apiVersion}/x -> /api/v1/x
            });

        // One built-in OpenAPI document per version. The built-in generator scopes a named
        // document to endpoints whose ApiDescription.GroupName matches the document name
        // ("v1"), which the API explorer sets above — so filtering is automatic; the
        // transformer only fills in document Info (title/version).
        foreach (var version in ApiVersions.All)
        {
            var documentName = $"v{version.MajorVersion}";
            services.AddOpenApi(documentName, options =>
            {
                options.AddDocumentTransformer<VersionedOpenApiDocumentTransformer>();
                options.AddOperationTransformer<VersionedOpenApiOperationTransformer>();
            });
        }

        return services;
    }
}
