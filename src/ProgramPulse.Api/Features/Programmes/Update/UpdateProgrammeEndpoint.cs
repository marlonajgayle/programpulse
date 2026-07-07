using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Programmes.Update;

/// <summary>
/// Updates an existing Programme. The id comes from the route; the remaining fields
/// come from the request body.
/// </summary>
public sealed class UpdateProgrammeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("programmes/{id:guid}", async (
            Guid id,
            UpdateProgrammeCommand command,
            UpdateProgrammeCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command with { Id = id }, cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<UpdateProgrammeCommand>()
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("UpdateProgramme")
        .WithTags("Programmes");
    }
}
