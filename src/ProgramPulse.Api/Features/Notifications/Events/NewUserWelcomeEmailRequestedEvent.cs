using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Features.Notifications.Events;

/// <summary>
/// Raised when an administrator provisions a new user. Carries the generated
/// temporary password so the user can sign in. Enqueued into the outbox and
/// handled asynchronously by <c>SendNewUserWelcomeEmailHandler</c>.
/// </summary>
public sealed record NewUserWelcomeEmailRequestedEvent(
    string To,
    string FirstName,
    string TemporaryPassword,
    DateTime OccurredOnUtc) : IDomainEvent;
