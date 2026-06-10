using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramPulse.Api.Domain.Entities.Users;

namespace ProgramPulse.Api.Infrastructure.Persistence.Configurations;

/// <summary>
/// FluentAPI mapping for <see cref="RefreshToken"/>. Picked up automatically by
/// <c>ApplyConfigurationsFromAssembly</c> in <see cref="ApplicationDbContext"/>.
/// </summary>
public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id)
            .IsClustered(false);

        builder.Property(rt => rt.Id)
            .ValueGeneratedOnAdd();

        builder.Property(rt => rt.JwtId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        builder.Property(rt => rt.RevokedAt)
            .IsRequired(false);

        builder.Property(rt => rt.ReplacedByToken)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(rt => rt.ReasonRevoked)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(rt => rt.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
