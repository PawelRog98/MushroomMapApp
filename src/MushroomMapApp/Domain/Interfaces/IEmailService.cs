using MushroomMapApp.Domain.Models;

namespace MushroomMapApp.Domain.Interfaces;

public interface IEmailService
{
    Task SendMessage(EmailMessage message, CancellationToken cancellationToken = default);
}
