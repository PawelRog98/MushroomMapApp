namespace MushroomMapApp.Features.Jobs.Triggered.Interfaces;

public interface ISendVerificationJob
{
    Task Execute(string email, string code);
}
