namespace MushroomMapApp.Domain.Exceptions;

public class SuspendedUserException : Exception
{
    public SuspendedUserException(DateTime suspendDate) : base($"User is suspended until: {suspendDate:yyyy-MM-dd HH:mm:ss}") { }
}
