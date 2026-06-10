using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Infrastructure.Messaging.Outbox;

/// <summary>
/// A persisted side effect captured inside the same transaction as the work
/// that produced it (the transactional outbox pattern). Dispatched later by
/// <see cref="OutboxProcessor"/>, guaranteeing at-least-once delivery.
/// </summary>
public sealed class OutboxMessage : BaseEntity<Guid>
{
    // Parameterless constructor for EF Core.
    internal OutboxMessage() { }

    public string Type { get; set; } = default!;
    public string Payload { get; set; } = default!; // JSON serialized payload
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }

    public static OutboxMessage Create(
        string type,
        string payload,
        DateTime occurredOnUtc)
    {
        return new OutboxMessage
        {
            Id = Guid.CreateVersion7(),
            Type = type,
            Payload = payload,
            OccurredOnUtc = occurredOnUtc
        };
    }
}
