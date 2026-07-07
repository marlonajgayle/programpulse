using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;

namespace ProgramPulse.Api.Infrastructure.Persistence.Configurations;

/// <summary>
/// FluentAPI mapping for <see cref="Objective"/>. Picked up automatically by
/// <c>ApplyConfigurationsFromAssembly</c> in <see cref="ApplicationDbContext"/>.
/// The soft-delete query filter is applied globally for all
/// <c>ISoftDeletable</c> entities, so it is intentionally not configured here.
/// Owns the one-to-one relationship to <c>Kpi</c> (a UNIQUE index on
/// <c>Kpi.ObjectiveId</c> enforces the single-KPI invariant). The relationship
/// back to <c>Programme</c> is configured in <see cref="ProgrammeConfiguration"/>.
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

        builder.HasOne(o => o.Kpi)
            .WithOne(k => k.Objective)
            .HasForeignKey<Kpi>(k => k.ObjectiveId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
