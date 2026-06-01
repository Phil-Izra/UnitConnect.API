using Microsoft.Extensions.Logging;
using UnitConnect.Application.Abstractions;

namespace UnitConnect.Infrastructure.Services;

public sealed class PushNotificationService(ILogger<PushNotificationService> logger) : IPushNotificationService
{
    public Task SendToComplexAsync(Guid complexId, string title, string body, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Sending push notification to complex {ComplexId}: {Title}", complexId, title);
        return Task.CompletedTask;
    }
}
