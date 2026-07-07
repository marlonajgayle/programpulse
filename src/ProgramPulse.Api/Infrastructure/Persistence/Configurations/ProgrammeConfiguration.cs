using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramPulse.Api.Domain.Entities.Tenants.Initiatives;

namespace ProgramPulse.Api.Infrastructure.Persistence.Configurations;

/// <summary>
/// FluentAPI mapping for <see cref="Initiative"/>. Picked up automatically by
/// <c>ApplyConfigurationsFromAssembly</c> in <see cref="ApplicationDbContext"/>.
/// The soft-delete query filter is applied globally for all
/// <c>ISoftDeletable</c> entities, so it is intentionally not configured here.
/// Owns the one-to-many relationship to <c>Objective</c>.
/// </summary>
public sealed class InitiativeConfiguration : IEntityTypeConfiguration<Initiative>
{
    public void Configure(EntityTypeBuilder<Initiative> builder)
    {
        builder.ToTable("Initiatives");

        builder.HasKey(i => i.Id)
            .IsClustered(false);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(i => i.StartDate)
            .IsRequired();

        builder.Property(i => i.EndDate)
            .IsRequired(false);

        builder.Property(i => i.CreatedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(i => i.CreatedDate)
            .IsRequired();

        builder.Property(i => i.LastModifiedBy)
            .IsRequired(false)
            .HasMaxLength(450);

        builder.Property(i => i.LastModifiedDate)
            .IsRequired(false);

        builder.Property(i => i.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(i => i.DeletedUtc)
            .IsRequired(false);

        builder.HasOne(i => i.Tenant)
            .WithMany()
            .HasForeignKey(i => i.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Objectives)
            .WithOne(o => o.Initiative)
            .HasForeignKey(o => o.InitiativeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Initiative.Objectives))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
