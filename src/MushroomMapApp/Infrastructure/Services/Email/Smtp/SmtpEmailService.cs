using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Domain.Models;

namespace MushroomMapApp.Infrastructure.Services.Email.Smtp;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(EmailSettings emailSettings, ILogger<SmtpEmailService> logger)
    {
        _emailSettings = emailSettings;
        _logger = logger;
    }


    public async Task SendMessage(EmailMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));

            email.To.Add(MailboxAddress.Parse(message.To));
            email.Subject = message.Subject;

            var buider = new BodyBuilder
            {
                HtmlBody = message.HtmlBody,
                TextBody = message.Body
            };

            email.Body = buider.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls,
                cancellationToken);

            await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password, cancellationToken);

            await smtp.SendAsync(email, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email to: {message.To}");
            throw;
        }
    }
}
