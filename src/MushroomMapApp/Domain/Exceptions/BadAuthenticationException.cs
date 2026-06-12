using Microsoft.AspNetCore.Http;

namespace MushroomMapApp.Domain.Exceptions;

public sealed class BadAuthenticationException : MainHttpException
{
    public BadAuthenticationException(string message) : base(message, StatusCodes.Status401Unauthorized) { }
}
