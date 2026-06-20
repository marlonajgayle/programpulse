using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Kpis.Create;

/// <summary>
/// Creates a new KPI under the given Objective.
/// </summary>
public sealed class CreateKpiEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("objectives/{objectiveId:guid}/kpis", async (
            Guid objectiveId,
            CreateKpiCommand command,
            CreateKpiCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                command with { ObjectiveId = objectiveId }, cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<CreateKpiCommand>()
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("CreateKpi")
        .WithTags("KPIs");
    }
}
