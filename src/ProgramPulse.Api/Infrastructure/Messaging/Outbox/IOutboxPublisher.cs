namespace ProgramPulse.Api.Infrastructure.Messaging.Outbox;

/// <summary>
/// Enqueues a message into the outbox within the caller's unit of work. The
/// message is persisted when the caller commits its transaction, never on its
/// own — this is what makes delivery transactional.
/// </summary>
public interface IOutboxPublisher
{
    void Add(string type, object payload);
}
