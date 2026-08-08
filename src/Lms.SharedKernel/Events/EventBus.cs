using Microsoft.Extensions.DependencyInjection;

namespace Lms.SharedKernel.Events;

/// <summary>
/// Something that happened, published by the module that owns the data.
/// Publishers do not know who subscribes — see artifacts/design/01-architecture.md §4.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}

public interface IEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct);
}

/// <summary>
/// Cross-module reactions. Distinct from <c>ICommandHandler</c> on purpose: publish/subscribe,
/// many handlers, no return value. Conflating the two is how mediator code becomes unreadable.
/// </summary>
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct = default)
        where TEvent : IDomainEvent;
}

/// <summary>
/// In-process dispatch. One process, so this is enough; the interface is what makes a
/// later move to Service Bus a single implementation.
/// </summary>
public sealed class InProcessEventBus(IServiceProvider services) : IEventBus
{
    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct = default)
        where TEvent : IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        using var scope = services.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IEventHandler<TEvent>>();

        foreach (var handler in handlers)
        {
            await handler.HandleAsync(domainEvent, ct);
        }
    }
}
