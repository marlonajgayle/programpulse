using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using ProgramPulse.Api.SharedKernel.OpenApi;

namespace ProgramPulse.Api.SharedKernel.Versioning;

internal sealed class VersionedOpenApiDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = ApiDocsConstants.Title,
            Version = context.DocumentName, // "v1"
        };

        var components = document.Components ?? new OpenApiComponents();
        document.Components = components;
        components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        components.SecuritySchemes[ApiDocsConstants.BearerScheme] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = ApiDocsConstants.BearerDescription,
        };

        return Task.CompletedTask;
    }
}
