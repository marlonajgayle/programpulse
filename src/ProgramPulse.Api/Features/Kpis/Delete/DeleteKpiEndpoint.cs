using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Kpis.Delete;

/// <summary>
/// Soft-deletes an existing KPI. The id comes from the route.
/// </summary>
public sealed class DeleteKpiEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("kpis/{id:guid}", async (
            Guid id,
            DeleteKpiCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new DeleteKpiCommand(id), cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("DeleteKpi")
        .WithTags("KPIs");
    }
}
