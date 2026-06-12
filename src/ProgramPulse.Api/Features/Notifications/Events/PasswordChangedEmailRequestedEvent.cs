using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Features.Notifications.Events;

/// <summary>
/// Raised when a password-change confirmation email should be sent. Enqueued into the outbox
/// and handled asynchronously by <c>SendPasswordChangedEmailHandler</c>.
/// </summary>
public sealed record PasswordChangedEmailRequestedEvent(
    string To,
    string FirstName,
    DateTime OccurredOnUtc) : IDomainEvent;
