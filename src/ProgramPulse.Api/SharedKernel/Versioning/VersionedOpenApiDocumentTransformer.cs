using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

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
            Title = "ProgramPulse API",
            Version = context.DocumentName, // "v1"
        };

        return Task.CompletedTask;
    }
}
