using Microsoft.AspNetCore.Http;

namespace MushroomMapApp.Domain.Exceptions;

public sealed class BadRequestException : MainHttpException
{
    public BadRequestException(string message) : base(message, StatusCodes.Status400BadRequest) { }
}
