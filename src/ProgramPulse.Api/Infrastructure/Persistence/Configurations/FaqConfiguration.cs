using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramPulse.Api.Domain.Entities.Faqs;

namespace ProgramPulse.Api.Infrastructure.Persistence.Configurations;

/// <summary>
/// FluentAPI mapping for <see cref="Faq"/>. Picked up automatically by
/// <c>ApplyConfigurationsFromAssembly</c> in <see cref="ApplicationDbContext"/>.
/// The soft-delete query filter is applied globally for all
/// <c>ISoftDeletable</c> entities, so it is intentionally not configured here.
/// </summary>
public sealed class FaqConfiguration : IEntityTypeConfiguration<Faq>
{
    public void Configure(EntityTypeBuilder<Faq> builder)
    {
        builder.ToTable("Faqs");

        builder.HasKey(f => f.Id)
            .IsClustered(false);

        builder.Property(f => f.Id)
            .ValueGeneratedNever();

        builder.Property(f => f.Question)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(f => f.Answer)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(f => f.CreatedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(f => f.CreatedDate)
            .IsRequired();

        builder.Property(f => f.LastModifiedBy)
            .IsRequired(false)
            .HasMaxLength(450);

        builder.Property(f => f.LastModifiedDate)
            .IsRequired(false);

        builder.Property(f => f.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(f => f.DeletedUtc)
            .IsRequired(false);
    }
}
