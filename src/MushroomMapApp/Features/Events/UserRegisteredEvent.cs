using Hangfire;
using MediatR;
using MushroomMapApp.Features.Jobs.Triggered.Interfaces;

namespace MushroomMapApp.Features.Events;

public record UserRegisteredEvent(string Email, string VerificationCode) : INotification;

public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly IBackgroundJobClient _jobs;

    public  UserRegisteredEventHandler(IBackgroundJobClient jobs)
    {
        _jobs = jobs;
    }

    public Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        _jobs.Enqueue<ISendVerificationJob>(x => x.Execute(notification.Email, notification.VerificationCode));

        return Task.CompletedTask;
    }
}
