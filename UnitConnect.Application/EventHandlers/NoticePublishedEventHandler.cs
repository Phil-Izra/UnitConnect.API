using UnitConnect.Application.Abstractions;
using UnitConnect.Domain.Enums;
using UnitConnect.Domain.Events;

namespace UnitConnect.Application.EventHandlers;

public sealed class NoticePublishedEventHandler(IPushNotificationService pushNotifications)
    : IDomainEventHandler<NoticePublishedEvent>
{
    public async Task HandleAsync(NoticePublishedEvent domainEvent, CancellationToken ct = default)
    {
        if (domainEvent.Urgency == NoticeUrgency.Urgent)
        {
            await pushNotifications.SendToComplexAsync(
                domainEvent.ComplexId,
                "Urgent Notice",
                "A new urgent notice has been posted. Please check the app.",
                ct);
        }
    }
}
