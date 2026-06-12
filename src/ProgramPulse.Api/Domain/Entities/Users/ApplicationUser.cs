using Microsoft.AspNetCore.Identity;
using ProgramPulse.Api.Domain.Entities.Tenants;

namespace ProgramPulse.Api.Domain.Entities.Users;

/// <summary>
/// Application user backed by ASP.NET Core Identity (string primary key,
/// default Identity schema). Extend with profile fields as features require.
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
