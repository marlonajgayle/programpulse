using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Kpis.List;

/// <summary>
/// Returns the single KPI belonging to the given Objective.
/// </summary>
public sealed class GetObjectiveKpiEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("objectives/{objectiveId:guid}/kpi", async (
            Guid objectiveId,
            GetObjectiveKpiQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetObjectiveKpiQuery(objectiveId), cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("GetObjectiveKpi")
        .WithTags("KPIs");
    }
}
