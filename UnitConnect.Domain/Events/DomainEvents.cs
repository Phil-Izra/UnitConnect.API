using UnitConnect.Domain.Common;
using UnitConnect.Domain.Enums;

namespace UnitConnect.Domain.Events;

// ── Complex ────────────────────────────────────────────────────────────────

public sealed record ComplexCreatedEvent(
    Guid ComplexId,
    string ComplexName) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

// ── Resident ───────────────────────────────────────────────────────────────

public sealed record ResidentRegisteredEvent(
    Guid ResidentId,
    Guid ComplexId,
    string Email) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record ResidentPasswordResetEvent(
    Guid ResidentId,
    Guid ComplexId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

// ── Listing ────────────────────────────────────────────────────────────────

public sealed record ListingCreatedEvent(
    Guid ListingId,
    Guid ComplexId,
    Guid SellerId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record ListingContactRequestedEvent(
    Guid ListingId,
    Guid SellerId,
    Guid BuyerId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record ListingSoldEvent(
    Guid ListingId,
    Guid ComplexId,
    Guid SellerId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

// ── Notice ─────────────────────────────────────────────────────────────────

public sealed record NoticePublishedEvent(
    Guid NoticeId,
    Guid ComplexId,
    NoticeUrgency Urgency) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
