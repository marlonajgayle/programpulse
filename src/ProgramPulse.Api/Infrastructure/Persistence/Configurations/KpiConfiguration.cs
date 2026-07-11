using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;

namespace ProgramPulse.Api.Infrastructure.Persistence.Configurations;

/// <summary>
/// FluentAPI mapping for <see cref="Kpi"/>. Picked up automatically by
/// <c>ApplyConfigurationsFromAssembly</c> in <see cref="ApplicationDbContext"/>.
/// The soft-delete query filter is applied globally for all
/// <c>ISoftDeletable</c> entities, so it is intentionally not configured here.
/// Owns the one-to-many relationship to <c>Measurement</c>. The relationship
/// back to <c>Objective</c> is configured in <see cref="ObjectiveConfiguration"/>.
/// </summary>
public sealed class KpiConfiguration : IEntityTypeConfiguration<Kpi>
{
    public void Configure(EntityTypeBuilder<Kpi> builder)
    {
        builder.ToTable("Kpis");

        builder.HasKey(k => k.Id)
            .IsClustered(false);

        builder.Property(k => k.Id)
            .ValueGeneratedNever();

        builder.Property(k => k.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(k => k.Unit)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(k => k.Direction)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(k => k.BaselineValue)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(k => k.TargetValue)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(k => k.CurrentValue)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(k => k.DueDate)
            .IsRequired();

        builder.Property(k => k.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(k => k.MeasurementFrequency)
            .IsRequired(false)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(k => k.Strategies)
            .IsRequired(false)
            .HasMaxLength(2000);

        builder.Property(k => k.Activities)
            .IsRequired(false)
            .HasMaxLength(2000);

        builder.Property(k => k.KeyOutputs)
            .IsRequired(false)
            .HasMaxLength(2000);

        builder.Property(k => k.PerformanceMeasure)
            .IsRequired(false)
            .HasMaxLength(2000);

        builder.Property(k => k.CreatedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(k => k.CreatedDate)
            .IsRequired();

        builder.Property(k => k.LastModifiedBy)
            .IsRequired(false)
            .HasMaxLength(450);

        builder.Property(k => k.LastModifiedDate)
            .IsRequired(false);

        builder.Property(k => k.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(k => k.DeletedUtc)
            .IsRequired(false);

        builder.HasMany(k => k.Measurements)
            .WithOne(m => m.Kpi)
            .HasForeignKey(m => m.KpiId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Kpi.Measurements))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
