using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProgramPulse.Api.Domain.Entities.Faqs;
using ProgramPulse.Api.Domain.Entities.Tenants;
using ProgramPulse.Api.Domain.Entities.Users;
using ProgramPulse.Api.Infrastructure.Messaging.Outbox;

namespace ProgramPulse.Api.Infrastructure.Persistence;

/// <summary>
/// Abstraction over <see cref="ApplicationDbContext"/> exposing only the
/// persistence operations that application/feature code should depend on.
/// </summary>
public interface IApplicationDbContext
{
    // Entity DbSets are added here as aggregates are introduced, e.g.:
    // DbSet<Project> Projects { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<OutboxMessage> OutboxMessages { get; }

    DbSet<Faq> Faqs { get; }

    DbSet<Tenant> Tenants { get; }

    void Attach<TEntity>(TEntity entity) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(
        IDbContextTransaction transaction,
        CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(
        IDbContextTransaction transaction,
        CancellationToken cancellationToken = default);

    IExecutionStrategy CreateExecutionStrategy();
}
