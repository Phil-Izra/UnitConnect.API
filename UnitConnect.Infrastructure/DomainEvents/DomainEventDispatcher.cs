using Microsoft.Extensions.DependencyInjection;
using UnitConnect.Application.Abstractions;
using UnitConnect.Application.EventHandlers;
using UnitConnect.Domain.Common;

namespace UnitConnect.Infrastructure.DomainEvents;

public sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
        var handlers = serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            if (handler is null) continue;
            var method = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;
            await (Task)method.Invoke(handler, [domainEvent, ct])!;
        }
    }
}
