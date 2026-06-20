using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Objectives.Update;

/// <summary>
/// Updates an existing Objective. The id comes from the route.
/// </summary>
public sealed class UpdateObjectiveEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("objectives/{id:guid}", async (
            Guid id,
            UpdateObjectiveCommand command,
            UpdateObjectiveCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command with { Id = id }, cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<UpdateObjectiveCommand>()
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("UpdateObjective")
        .WithTags("Objectives");
    }
}
