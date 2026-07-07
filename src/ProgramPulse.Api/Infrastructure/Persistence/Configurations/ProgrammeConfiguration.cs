using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;

namespace ProgramPulse.Api.Infrastructure.Persistence.Configurations;

/// <summary>
/// FluentAPI mapping for <see cref="Programme"/>. Picked up automatically by
/// <c>ApplyConfigurationsFromAssembly</c> in <see cref="ApplicationDbContext"/>.
/// The soft-delete query filter is applied globally for all
/// <c>ISoftDeletable</c> entities, so it is intentionally not configured here.
/// Owns the one-to-many relationship to <c>Objective</c>.
/// </summary>
public sealed class ProgrammeConfiguration : IEntityTypeConfiguration<Programme>
{
    public void Configure(EntityTypeBuilder<Programme> builder)
    {
        builder.ToTable("Programmes");

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
            .IsRequired(false);

        builder.Property(i => i.EndDate)
            .IsRequired(false);

        // Status is derived from EndDate at read time — never persisted.
        builder.Ignore(i => i.Status);

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

        // Optional self-reference to a parent programme. Restrict on delete to
        // avoid a multiple-cascade-path cycle on SQL Server.
        builder.HasOne(i => i.ParentProgramme)
            .WithMany()
            .HasForeignKey(i => i.ParentProgrammeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Objectives)
            .WithOne(o => o.Programme)
            .HasForeignKey(o => o.ProgrammeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Programme.Objectives))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
