using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Features.Notifications.Events;

/// <summary>
/// Raised when an email-confirmation message should be sent (e.g. on user creation). Enqueued
/// into the outbox and handled asynchronously by <c>SendEmailConfirmationEmailHandler</c>.
/// </summary>
public sealed record EmailConfirmationRequestedEvent(
    string To,
    string FirstName,
    string ConfirmLink,
    DateTime OccurredOnUtc) : IDomainEvent;
