using Microsoft.Extensions.Logging;
using UnitConnect.Application.Abstractions;

namespace UnitConnect.Infrastructure.Services;

public sealed class EmailService(ILogger<EmailService> logger) : IEmailService
{
    public Task SendWelcomeEmailAsync(string toEmail, string firstName, CancellationToken ct = default)
    {
        logger.LogInformation("Sending welcome email to {Email}", toEmail);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken ct = default)
    {
        logger.LogInformation("Sending password reset email to {Email}", toEmail);
        return Task.CompletedTask;
    }

    public Task SendResidentInviteEmailAsync(string toEmail, string complexName, string inviteToken, CancellationToken ct = default)
    {
        logger.LogInformation("Sending invite email to {Email} for complex {Complex}", toEmail, complexName);
        return Task.CompletedTask;
    }
}
