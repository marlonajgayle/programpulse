using Microsoft.AspNetCore.Identity;

namespace ProgramPulse.Api.Domain.Entities.Users;

/// <summary>
/// Application user backed by ASP.NET Core Identity (string primary key,
/// default Identity schema). Extend with profile fields as features require.
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
}
