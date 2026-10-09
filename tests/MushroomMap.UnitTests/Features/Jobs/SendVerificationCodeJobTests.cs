using FluentAssertions;
using Moq;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Domain.Models;
using MushroomMapApp.Features.Jobs.Triggered;
using Xunit;

namespace MushroomMap.UnitTests.Features.Jobs;

public class SendVerificationCodeJobTests
{
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly SendVerificationCodeJob _job;

    public SendVerificationCodeJobTests()
    {
        _emailServiceMock = new Mock<IEmailService>();
        _job = new SendVerificationCodeJob(_emailServiceMock.Object);
    }

    [Fact]
    public async Task Execute_SendsEmailWithCode()
    {
        var email = "test@test.com";
        var code = "ABC123";

        await _job.Execute(email, code);

        _emailServiceMock.Verify(
            x => x.SendMessage(
                It.Is<EmailMessage>(m =>
                    m.To == email &&
                    m.Subject == "Verification Code" &&
                    m.HtmlBody!.Contains(code)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_IncludesCodeInHtmlBody()
    {
        var email = "user@example.com";
        var code = "XYZ789";

        await _job.Execute(email, code);

        _emailServiceMock.Verify(
            x => x.SendMessage(
                It.Is<EmailMessage>(m => m.HtmlBody!.Contains(code)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_SetsCorrectRecipient()
    {
        var email = "recipient@test.com";
        var code = "CODE";

        await _job.Execute(email, code);

        _emailServiceMock.Verify(
            x => x.SendMessage(
                It.Is<EmailMessage>(m => m.To == email),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_SetsCorrectSubject()
    {
        await _job.Execute("test@test.com", "CODE");

        _emailServiceMock.Verify(
            x => x.SendMessage(
                It.Is<EmailMessage>(m => m.Subject == "Verification Code"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
