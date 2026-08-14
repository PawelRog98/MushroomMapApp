using MediatR;
using MushroomMapApp.Features.Jobs.Abstraction;
using MushroomMapApp.Features.Users.Unsuspend;

namespace MushroomMapApp.Features.Jobs.Recurring;

[RecurringJob("unsuspend-expired-suspensions", "0 0 * * *")]
public class UnsuspendExpiredSuspensionsJob : IRecurringJob
{
    private readonly IMediator _mediator;

    public UnsuspendExpiredSuspensionsJob(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task ExecuteJob(CancellationToken cancellationToken)
    {
        await _mediator.Send(new UnsuspendExpiredSuspensionsCommand(), cancellationToken);
    }
}
