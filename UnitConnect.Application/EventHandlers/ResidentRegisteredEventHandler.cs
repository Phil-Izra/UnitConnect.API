using UnitConnect.Application.Abstractions;
using UnitConnect.Domain.Events;

namespace UnitConnect.Application.EventHandlers;

public sealed class ResidentRegisteredEventHandler(IEmailService emailService)
    : IDomainEventHandler<ResidentRegisteredEvent>
{
    public async Task HandleAsync(ResidentRegisteredEvent domainEvent, CancellationToken ct = default)
    {
        await emailService.SendWelcomeEmailAsync(domainEvent.Email, domainEvent.Email, ct);
    }
}
