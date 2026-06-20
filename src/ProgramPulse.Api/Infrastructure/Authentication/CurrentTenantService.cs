using Microsoft.AspNetCore.Identity;
using ProgramPulse.Api.Domain.Entities.Users;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Infrastructure.Authentication;

/// <summary>
/// Resolves the caller's tenant from their authenticated identity, mirroring the
/// lookup used by <c>AddUserCommandHandler</c>: the JWT subject identifies the user,
/// whose <see cref="ApplicationUser.TenantId"/> determines the tenant scope.
/// </summary>
public sealed class CurrentTenantService(
    ICurrentUser currentUser,
    UserManager<ApplicationUser> userManager) : ICurrentTenant
{
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result<Guid>> GetTenantIdAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return Result<Guid>.Failure(AuthenticationErrors.UserNotAuthenticated());
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user?.TenantId is null)
        {
            return Result<Guid>.Failure(UserManagementErrors.ActingAdminHasNoTenant());
        }

        return Result<Guid>.Success(user.TenantId.Value);
    }
}
