using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramPulse.Api.Domain.Entities.Tenants;

namespace ProgramPulse.Api.Infrastructure.Persistence.Configurations;

/// <summary>
/// FluentAPI mapping for <see cref="Tenant"/>. Picked up automatically by
/// <c>ApplyConfigurationsFromAssembly</c> in <see cref="ApplicationDbContext"/>.
/// The soft-delete query filter is applied globally for all
/// <c>ISoftDeletable</c> entities, so it is intentionally not configured here.
/// Owns the one-to-many relationship to <c>ApplicationUser</c>.
/// </summary>
public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id)
            .IsClustered(false);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(t => t.Slug)
            .IsUnique();

        builder.Property(t => t.Description)
            .IsRequired(false)
            .HasMaxLength(1000);

        builder.Property(t => t.CreatedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(t => t.CreatedDate)
            .IsRequired();

        builder.Property(t => t.LastModifiedBy)
            .IsRequired(false)
            .HasMaxLength(450);

        builder.Property(t => t.LastModifiedDate)
            .IsRequired(false);

        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.DeletedUtc)
            .IsRequired(false);

        builder.HasMany(t => t.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Metadata
            .FindNavigation(nameof(Tenant.Users))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
