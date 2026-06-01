using UnitConnect.Domain.Common;

namespace UnitConnect.Application.EventHandlers;

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct = default);
}
