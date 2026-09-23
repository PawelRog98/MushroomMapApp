using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Domain.Models;
using MushroomMapApp.Features.Jobs.Triggered.Interfaces;

namespace MushroomMapApp.Features.Jobs.Triggered;

public class SendVerificationCodeJob : ISendVerificationJob
{
    private readonly IEmailService _emailService;

    public  SendVerificationCodeJob(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Execute(string email, string code)
    {
        var html =
            $"""
             <div align="center">
                 <h1>Mushroom Map</h1>
                 <hr width="60%">
                 <h2>Account Verification</h2>
                 <p>Welcome! To complete your registration, please enter the following verification code in the application:</p>
                 <p>
                    <font size="6"><b>{code}</b></font>
                 </p>
                 <p><i>This code will expire in 3 hours.</i></p>
                 <hr width="60%">
                 <p><small>If you did not create an account with Mushroom Map, please ignore this email.</small></p>
             </div>
             """;

        await _emailService.SendMessage(new EmailMessage
        {
            To = email,
            Subject = "Verification Code",
            HtmlBody = html
        });
    }
}
