using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Domain.Models;

namespace MushroomMap.IntegrationTests.Fixtures;

public class FakeEmailService : IEmailService
{
    private readonly List<EmailMessage> _sent = new();

    public IReadOnlyList<EmailMessage> Sent
    {
        get
        {
            lock (_sent)
            {
                return _sent.ToArray();
            }
        }
    }

    public Task SendMessage(EmailMessage message, CancellationToken cancellationToken = default)
    {
        lock (_sent)
        {
            _sent.Add(message);
        }

        return Task.CompletedTask;
    }
}
