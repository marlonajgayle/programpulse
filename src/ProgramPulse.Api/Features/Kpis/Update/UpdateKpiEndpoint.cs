using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Kpis.Update;

/// <summary>
/// Updates an existing KPI. The id comes from the route.
/// </summary>
public sealed class UpdateKpiEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("kpis/{id:guid}", async (
            Guid id,
            UpdateKpiCommand command,
            UpdateKpiCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command with { Id = id }, cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<UpdateKpiCommand>()
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("UpdateKpi")
        .WithTags("KPIs");
    }
}
