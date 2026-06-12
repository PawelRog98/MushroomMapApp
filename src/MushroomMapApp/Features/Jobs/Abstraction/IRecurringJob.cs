namespace MushroomMapApp.Features.Jobs.Abstraction;

public interface IRecurringJob
{
    Task ExecuteJob(CancellationToken cancellationToken);
}