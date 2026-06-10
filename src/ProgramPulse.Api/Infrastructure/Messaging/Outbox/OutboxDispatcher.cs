using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Infrastructure.Messaging.Outbox;

/// <summary>
/// Resolves the handler(s) for a stored <see cref="OutboxMessage"/> by mapping
/// its <see cref="OutboxMessage.Type"/> string to the matching
/// <see cref="IDomainEvent"/> CLR type and invoking every registered
/// <see cref="IDomainEventHandler{T}"/>. Built once at startup as a singleton;
/// adding a new event requires no changes here.
/// </summary>
internal sealed class OutboxDispatcher
{
    private delegate Task DispatchDelegate(
        string payload,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);

    private readonly IReadOnlyDictionary<string, DispatchDelegate> _registry;

    public OutboxDispatcher()
    {
        var registry = new Dictionary<string, DispatchDelegate>(StringComparer.Ordinal);

        var eventTypes = typeof(Program).Assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && typeof(IDomainEvent).IsAssignableFrom(t));

        foreach (var eventType in eventTypes)
        {
            if (registry.ContainsKey(eventType.Name))
            {
                throw new InvalidOperationException(
                    $"Duplicate domain event name '{eventType.Name}'. Outbox message types " +
                    "are keyed by the event's simple name, which must be unique.");
            }

            registry.Add(eventType.Name, BuildDispatcher(eventType));
        }

        _registry = registry;
    }

    public Task DispatchAsync(
        OutboxMessage message,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        if (!_registry.TryGetValue(message.Type, out var dispatch))
        {
            throw new InvalidOperationException(
                $"No registration for outbox type '{message.Type}'.");
        }

        return dispatch(message.Payload, serviceProvider, cancellationToken);
    }

    private static DispatchDelegate BuildDispatcher(Type eventType)
    {
        var method = typeof(OutboxDispatcher)
            .GetMethod(nameof(DispatchTyped), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(eventType);

        return method.CreateDelegate<DispatchDelegate>();
    }

    private static async Task DispatchTyped<TEvent>(
        string payload,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
        where TEvent : IDomainEvent
    {
        var domainEvent = JsonSerializer.Deserialize<TEvent>(payload)
            ?? throw new InvalidOperationException(
                $"Failed to deserialize outbox payload to '{typeof(TEvent).Name}'.");

        var handlers = serviceProvider.GetServices<IDomainEventHandler<TEvent>>();

        foreach (var handler in handlers)
            await handler.HandleAsync(domainEvent, cancellationToken);
    }
}
