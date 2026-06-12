namespace MushroomMapApp.Domain.Exceptions;

public class NotActiveUserException : Exception
{
    public NotActiveUserException() : base("User is not active") { }
}
