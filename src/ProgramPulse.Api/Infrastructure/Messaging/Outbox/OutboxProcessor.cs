using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Infrastructure.Persistence;

namespace ProgramPulse.Api.Infrastructure.Messaging.Outbox;

/// <summary>
/// Polls the outbox table and dispatches unprocessed messages to their
/// <see cref="IDomainEventHandler{T}"/> via <see cref="OutboxDispatcher"/>.
/// Successful messages are stamped processed; failures record the error and
/// are retried on a later pass.
/// </summary>
internal sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    OutboxDispatcher dispatcher
) : BackgroundService
{
    private readonly Serilog.ILogger _logger = Serilog.Log
        .ForContext("LogSource", "Background")
        .ForContext("Job", "OutboxProcessor");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Outbox processing pass failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }

    private async Task ProcessAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var messages = await db.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(20)
            .ToListAsync(ct);

        if (messages.Count == 0)
            return;

        _logger.Information("Processing {Count} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                await dispatcher.DispatchAsync(message, scope.ServiceProvider, ct);
                message.ProcessedOnUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
                _logger.Error(ex, "OutboxMessage {MessageId} processing failed", message.Id);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
