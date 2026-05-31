namespace UnitConnect.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
