namespace MushroomMapApp.Domain.Exceptions;

public abstract class MainHttpException : Exception
{
    public virtual int StatusCode { get; set; }
    protected MainHttpException(string? message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}
