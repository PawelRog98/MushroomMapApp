using Microsoft.AspNetCore.Http;

namespace MushroomMapApp.Domain.Exceptions;

public sealed class NotFoundException : MainHttpException
{
    public NotFoundException(string message) : base(message, StatusCodes.Status404NotFound) { }
}
