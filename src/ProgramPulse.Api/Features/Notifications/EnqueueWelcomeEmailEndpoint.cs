using Asp.Versioning;
using ProgramPulse.Api.Features.Notifications.Events;
using ProgramPulse.Api.Infrastructure.Messaging.Outbox;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Notifications;

/// <summary>
/// Enqueues a welcome email into the outbox and returns immediately. The
/// background <c>OutboxProcessor</c> dispatches it to
/// <c>SendWelcomeEmailHandler</c> within ~15s — the reliable, transactional path.
/// </summary>
public sealed class EnqueueWelcomeEmailEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("notifications/welcome", async (
            EnqueueWelcomeEmailRequest request,
            IOutboxPublisher outboxPublisher,
            IApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var welcomeEvent = new WelcomeEmailRequestedEvent(
                request.To,
                request.UserName,
                DateTime.UtcNow);

            outboxPublisher.Add(nameof(WelcomeEmailRequestedEvent), welcomeEvent);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Accepted();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithName("EnqueueWelcomeEmail")
        .WithTags("Notifications");
    }
}

public sealed record EnqueueWelcomeEmailRequest(
    string To,
    string UserName);
