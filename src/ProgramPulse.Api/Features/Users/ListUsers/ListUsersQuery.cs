using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Users;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Users.ListUsers;

public sealed record ListUsersQuery;

public sealed record UserListItemResponse(
    string Id,
    string? FirstName,
    string? LastName,
    string Email,
    IReadOnlyList<string> Roles,
    bool EmailConfirmed,
    bool IsLockedOut,
    DateTime? LastLoginAt);

/// <summary>
/// Admin-only query that returns every user in the acting administrator's tenant
/// (name, email, roles and the fields the UI derives a status from). Uses
/// <see cref="UserManager{TUser}"/> because the Identity user/role tables are not
/// exposed on <c>IApplicationDbContext</c>.
/// </summary>
public sealed class ListUsersQueryHandler(
    UserManager<ApplicationUser> userManager,
    ICurrentUser currentUser)
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<IReadOnlyList<UserListItemResponse>>> HandleAsync(
        ListUsersQuery query,
        CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId;
        if (adminId is null)
        {
            return Result<IReadOnlyList<UserListItemResponse>>.Failure(
                AuthenticationErrors.UserNotAuthenticated());
        }

        var admin = await _userManager.FindByIdAsync(adminId);
        if (admin?.TenantId is null)
        {
            return Result<IReadOnlyList<UserListItemResponse>>.Failure(
                UserManagementErrors.ActingAdminHasNoTenant());
        }

        var users = await _userManager.Users
            .AsNoTracking()
            .Where(u => u.TenantId == admin.TenantId)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var items = new List<UserListItemResponse>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            items.Add(new UserListItemResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email ?? string.Empty,
                roles.ToList(),
                user.EmailConfirmed,
                user.LockoutEnd is { } lockoutEnd && lockoutEnd > now,
                user.LastLoginAt));
        }

        return Result<IReadOnlyList<UserListItemResponse>>.Success(items);
    }
}
