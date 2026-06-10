using System.Text.Json;
using ProgramPulse.Api.Infrastructure.Persistence;

namespace ProgramPulse.Api.Infrastructure.Messaging.Outbox;

/// <summary>
/// Default <see cref="IOutboxPublisher"/>. Adds the message to the tracked
/// <see cref="IApplicationDbContext.OutboxMessages"/> set but does not save —
/// it rides along with the caller's <c>SaveChangesAsync</c>.
/// </summary>
internal sealed class OutboxPublisher(
    IApplicationDbContext dbContext
) : IOutboxPublisher
{
    public void Add(string type, object payload)
    {
        var message = OutboxMessage.Create(
            type,
            JsonSerializer.Serialize(payload),
            DateTime.UtcNow);

        dbContext.OutboxMessages.Add(message);
    }
}
