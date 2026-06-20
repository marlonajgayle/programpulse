using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Infrastructure.Authentication;

/// <summary>
/// Resolves the tenant the authenticated caller belongs to. Used by tenant-scoped
/// feature handlers to constrain every operation to the caller's own tenant.
/// </summary>
public interface ICurrentTenant
{
    /// <summary>
    /// Returns the current caller's tenant id, or a failure when the caller is not
    /// authenticated or is not associated with a tenant.
    /// </summary>
    Task<Result<Guid>> GetTenantIdAsync(CancellationToken cancellationToken);
}
