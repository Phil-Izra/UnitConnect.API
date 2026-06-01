using UnitConnect.Domain.Common;

namespace UnitConnect.Application.Abstractions;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}
