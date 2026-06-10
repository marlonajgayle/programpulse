using Microsoft.AspNetCore.Identity;

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
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
