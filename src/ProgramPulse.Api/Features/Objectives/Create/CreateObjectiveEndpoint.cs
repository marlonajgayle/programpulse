using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Objectives.Create;

/// <summary>
/// Creates a new Objective under the given Initiative.
/// </summary>
public sealed class CreateObjectiveEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("initiatives/{initiativeId:guid}/objectives", async (
            Guid initiativeId,
            CreateObjectiveCommand command,
            CreateObjectiveCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                command with { InitiativeId = initiativeId }, cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<CreateObjectiveCommand>()
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("CreateObjective")
        .WithTags("Objectives");
    }
}
