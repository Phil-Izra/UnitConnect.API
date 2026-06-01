namespace UnitConnect.Application.Abstractions;

public interface IPushNotificationService
{
    Task SendToComplexAsync(Guid complexId, string title, string body, CancellationToken ct = default);
}
