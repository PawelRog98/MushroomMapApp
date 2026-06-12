using Microsoft.AspNetCore.Http;

namespace MushroomMapApp.Domain.Exceptions;

public sealed class ForbiddenException : MainHttpException
{
    public ForbiddenException(string message) : base(message, StatusCodes.Status403Forbidden) { }
}
