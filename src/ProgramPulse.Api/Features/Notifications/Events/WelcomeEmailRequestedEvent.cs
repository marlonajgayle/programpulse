using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Features.Notifications.Events;

/// <summary>
/// Raised when a welcome email should be sent. Enqueued into the outbox and
/// handled asynchronously by <c>SendWelcomeEmailHandler</c>.
/// </summary>
public sealed record WelcomeEmailRequestedEvent(
    string To,
    string UserName,
    DateTime OccurredOnUtc) : IDomainEvent;
