using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Features.Notifications.Events;

/// <summary>
/// Raised when a password-reset email should be sent. Enqueued into the outbox and
/// handled asynchronously by <c>SendPasswordResetEmailHandler</c>.
/// </summary>
public sealed record PasswordResetEmailRequestedEvent(
    string To,
    string FirstName,
    string ResetLink,
    DateTime OccurredOnUtc) : IDomainEvent;
