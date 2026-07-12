using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;

namespace ProgramPulse.Api.Infrastructure.Persistence.Configurations;

/// <summary>
/// FluentAPI mapping for <see cref="Measurement"/>. Picked up automatically by
/// <c>ApplyConfigurationsFromAssembly</c> in <see cref="ApplicationDbContext"/>.
/// The soft-delete query filter is applied globally for all
/// <c>ISoftDeletable</c> entities, so it is intentionally not configured here.
/// The relationship back to <c>Kpi</c> is configured in <see cref="KpiConfiguration"/>.
/// </summary>
public sealed class MeasurementConfiguration : IEntityTypeConfiguration<Measurement>
{
    public void Configure(EntityTypeBuilder<Measurement> builder)
    {
        builder.ToTable("Measurements");

        builder.HasKey(m => m.Id)
            .IsClustered(false);

        builder.Property(m => m.Id)
            .ValueGeneratedNever();

        builder.Property(m => m.Value)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(m => m.Notes)
            .IsRequired(false)
            .HasMaxLength(1000);

        builder.Property(m => m.MeasurementDate)
            .IsRequired();

        builder.Property(m => m.CreatedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(m => m.CreatedDate)
            .IsRequired();

        builder.Property(m => m.LastModifiedBy)
            .IsRequired(false)
            .HasMaxLength(450);

        builder.Property(m => m.LastModifiedDate)
            .IsRequired(false);

        builder.Property(m => m.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(m => m.DeletedUtc)
            .IsRequired(false);

        builder.HasMany(m => m.Comments)
            .WithOne(c => c.Measurement)
            .HasForeignKey(c => c.MeasurementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Measurement.Comments))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
