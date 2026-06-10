using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramPulse.Api.Domain.Entities.Users;

namespace ProgramPulse.Api.Infrastructure.Persistence.Configurations;

/// <summary>
/// FluentAPI mapping for <see cref="ApplicationUser"/>. Demonstrates the
/// configuration convention picked up by <c>ApplyConfigurationsFromAssembly</c>.
/// </summary>
public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.UserName)
            .HasMaxLength(256);

        builder.Property(user => user.Email)
            .HasMaxLength(256);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.LastLoginAt)
            .IsRequired(false);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey("ApplicationUserId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}