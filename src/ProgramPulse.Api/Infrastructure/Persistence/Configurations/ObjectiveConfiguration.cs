using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramPulse.Api.Domain.Entities.Tenants.Initiatives;

namespace ProgramPulse.Api.Infrastructure.Persistence.Configurations;

/// <summary>
/// FluentAPI mapping for <see cref="Objective"/>. Picked up automatically by
/// <c>ApplyConfigurationsFromAssembly</c> in <see cref="ApplicationDbContext"/>.
/// The soft-delete query filter is applied globally for all
/// <c>ISoftDeletable</c> entities, so it is intentionally not configured here.
/// Owns the one-to-many relationship to <c>Kpi</c>. The relationship back to
/// <c>Initiative</c> is configured in <see cref="InitiativeConfiguration"/>.
/// </summary>
public sealed class ObjectiveConfiguration : IEntityTypeConfiguration<Objective>
{
    public void Configure(EntityTypeBuilder<Objective> builder)
    {
        builder.ToTable("Objectives");

        builder.HasKey(o => o.Id)
            .IsClustered(false);

        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(o => o.CreatedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(o => o.CreatedDate)
            .IsRequired();

        builder.Property(o => o.LastModifiedBy)
            .IsRequired(false)
            .HasMaxLength(450);

        builder.Property(o => o.LastModifiedDate)
            .IsRequired(false);

        builder.Property(o => o.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(o => o.DeletedUtc)
            .IsRequired(false);

        builder.HasMany(o => o.Kpis)
            .WithOne(k => k.Objective)
            .HasForeignKey(k => k.ObjectiveId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Objective.Kpis))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
