namespace UnitConnect.Application.Abstractions;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string toEmail, string firstName, CancellationToken ct = default);
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken ct = default);
    Task SendResidentInviteEmailAsync(string toEmail, string complexName, string inviteToken, CancellationToken ct = default);
}
