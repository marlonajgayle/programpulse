using Microsoft.Extensions.DependencyInjection.Extensions;
using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Infrastructure.Messaging.Outbox;

/// <summary>
/// Wires up the transactional outbox: the publisher, the dispatcher registry,
/// every <see cref="IDomainEventHandler{T}"/> in the assembly, and the polling
/// background processor.
/// </summary>
public static class OutboxConfiguration
{
    public static IServiceCollection AddOutboxMessaging(
        this IServiceCollection services)
    {
        services.AddScoped<IOutboxPublisher, OutboxPublisher>();
        services.AddSingleton<OutboxDispatcher>();

        AddDomainEventHandlers(services);

        services.AddHostedService<OutboxProcessor>();

        return services;
    }

    private static void AddDomainEventHandlers(IServiceCollection services)
    {
        var handlerInterface = typeof(IDomainEventHandler<>);

        var descriptors = typeof(Program).Assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType
                            && i.GetGenericTypeDefinition() == handlerInterface)
                .Select(i => ServiceDescriptor.Scoped(i, t)));

        services.TryAddEnumerable(descriptors);
    }
}
