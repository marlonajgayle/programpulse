using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using ProgramPulse.Api.SharedKernel.OpenApi;

namespace ProgramPulse.Api.SharedKernel.Versioning;

internal sealed class VersionedOpenApiOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        var hasAuth = metadata.OfType<IAuthorizeData>().Any();
        var hasAllowAnon = metadata.OfType<IAllowAnonymous>().Any();

        if (hasAuth && !hasAllowAnon)
        {
            var schemeRef = new OpenApiSecuritySchemeReference(
                ApiDocsConstants.BearerScheme,
                context.Document);

            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                { schemeRef, new List<string>() },
            });
        }

        return Task.CompletedTask;
    }
}
