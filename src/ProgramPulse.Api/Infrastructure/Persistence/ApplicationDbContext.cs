using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProgramPulse.Api.Domain.Entities.Users;
using ProgramPulse.Api.Infrastructure.Messaging.Outbox;
using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Infrastructure.Persistence;

/// <summary>
/// Primary EF Core context. Backs ASP.NET Core Identity and is the single
/// write/read surface for application aggregates. Entity mappings live in
/// <c>Infrastructure/Persistence/Configurations</c> and are applied via
/// <see cref="ModelBuilder.ApplyConfigurationsFromAssembly(System.Reflection.Assembly, Func{Type, bool})"/>.
/// </summary>
public sealed class ApplicationDbContext
    : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    // Fallback principal for audit stamping until an authenticated-user
    // context is wired in (Bearer auth is planned, see OpenApi config).
    private const string SystemUser = "system";

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        ApplySoftDeleteQueryFilters(builder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    void IApplicationDbContext.Attach<TEntity>(TEntity entity) => Attach(entity);

    public async Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default) =>
        await Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(
        IDbContextTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(
        IDbContextTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        await transaction.RollbackAsync(cancellationToken);
    }

    public IExecutionStrategy CreateExecutionStrategy() =>
        Database.CreateExecutionStrategy();

    private void ApplyAuditAndSoftDelete()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is IAuditableEntity auditable)
            {
                if (entry.State == EntityState.Added)
                    auditable.SetCreatedAuditInfo(SystemUser);
                else if (entry.State == EntityState.Modified)
                    auditable.SetModifiedAuditInfo(SystemUser);
            }

            if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletable softDeletable)
            {
                entry.State = EntityState.Modified;
                softDeletable.MarkAsDeleted();
            }
        }
    }

    private static void ApplySoftDeleteQueryFilters(ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var body = Expression.Equal(
                Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted)),
                Expression.Constant(false));

            builder.Entity(entityType.ClrType)
                .HasQueryFilter(Expression.Lambda(body, parameter));
        }
    }
}
