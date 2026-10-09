using FluentAssertions;
using Microsoft.Extensions.Logging;
using MushroomMap.UnitTests.Common.Logging;
using MushroomMapApp.Domain.Models;
using MushroomMapApp.Infrastructure.Services.Email.Smtp;
using Xunit;

namespace MushroomMap.UnitTests.Infrastructure.Services;

public class SmtpEmailServiceTests
{
    private readonly EmailSettings _settings = new()
    {
        Host = "smtp.test.com",
        Port = 587,
        Username = "user",
        Password = "pass",
        FromEmail = "from@test.com",
        FromName = "Test Sender"
    };

    [Fact]
    public async Task SendMessage_LogsError_AndRethrows_WhenConnectionFails()
    {
        var logger = new RecordingLogger<SmtpEmailService>();
        var service = new SmtpEmailService(_settings, logger);

        var message = new EmailMessage
        {
            To = "to@test.com",
            Subject = "Test",
            HtmlBody = "<p>Test</p>"
        };

        var act = () => service.SendMessage(message);

        await act.Should().ThrowAsync<Exception>();

        var entry = logger.Entries.Should().ContainSingle().Which;
        entry.Level.Should().Be(LogLevel.Error);
        entry.Exception.Should().NotBeNull();
    }

    [Fact]
    public async Task SendMessage_Throws_WhenSmtpServerUnavailable()
    {
        var logger = new RecordingLogger<SmtpEmailService>();
        var service = new SmtpEmailService(_settings, logger);

        var message = new EmailMessage
        {
            To = "to@test.com",
            Subject = "Test",
            HtmlBody = "<p>Test</p>"
        };

        var act = () => service.SendMessage(message);

        await act.Should().ThrowAsync<Exception>();
    }
}
