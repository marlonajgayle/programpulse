using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;

namespace ProgramPulse.Api.Infrastructure.Persistence.Configurations;

/// <summary>
/// FluentAPI mapping for <see cref="MeasurementComment"/>. Picked up automatically by
/// <c>ApplyConfigurationsFromAssembly</c> in <see cref="ApplicationDbContext"/>.
/// The soft-delete query filter is applied globally for all
/// <c>ISoftDeletable</c> entities, so it is intentionally not configured here.
/// The relationship back to <c>Measurement</c> is configured in <see cref="MeasurementConfiguration"/>.
/// </summary>
public sealed class MeasurementCommentConfiguration : IEntityTypeConfiguration<MeasurementComment>
{
    public void Configure(EntityTypeBuilder<MeasurementComment> builder)
    {
        builder.ToTable("MeasurementComments");

        builder.HasKey(c => c.Id)
            .IsClustered(false);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Text)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.CreatedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(c => c.CreatedDate)
            .IsRequired();

        builder.Property(c => c.LastModifiedBy)
            .IsRequired(false)
            .HasMaxLength(450);

        builder.Property(c => c.LastModifiedDate)
            .IsRequired(false);

        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.DeletedUtc)
            .IsRequired(false);
    }
}
