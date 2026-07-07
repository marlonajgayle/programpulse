using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Domain.Entities.Tenants.Initiatives;

public static class KpiErrors
{
    public static Error KpiNotFound(Guid kpiId) => Error.NotFound(
        code: "Kpi.NotFound",
        message: $"KPI with ID '{kpiId}' was not found.");
}
