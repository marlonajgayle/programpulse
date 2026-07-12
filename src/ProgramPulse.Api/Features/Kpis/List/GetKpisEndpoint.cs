using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Kpis.List;

/// <summary>
/// Returns the KPIs belonging to the given Objective.
/// </summary>
public sealed class GetObjectiveKpisEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("objectives/{objectiveId:guid}/kpis", async (
            Guid objectiveId,
            KpiCategory? category,
            GetObjectiveKpisQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetObjectiveKpisQuery(objectiveId, category), cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("GetObjectiveKpis")
        .WithTags("KPIs");
    }
}
