using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MushroomMap.UnitTests.Common.Logging;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Infrastructure.Middlewares;
using Xunit;

namespace MushroomMap.UnitTests.Infrastructure.Middlewares;

public class GlobalExceptionHandlingMiddlewareTests
{
    private readonly RecordingLogger<GlobalExceptionHandlingMiddleware> _logger = new();
    private readonly DefaultHttpContext _httpContext;
    private readonly MemoryStream _responseBody;

    public GlobalExceptionHandlingMiddlewareTests()
    {
        _responseBody = new MemoryStream();
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = _responseBody;
    }

    private Task InvokeAsync(RequestDelegate next)
        => new GlobalExceptionHandlingMiddleware(next, _logger).InvokeAsync(_httpContext);

    private JsonElement ReadResponseBody()
    {
        _responseBody.Position = 0;
        using var reader = new StreamReader(_responseBody);
        var json = reader.ReadToEnd();
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    [Fact]
    public async Task InvokeAsync_DoesNotHandleResponse_WhenNextDoesNotThrow()
    {
        await InvokeAsync(context =>
        {
            context.Response.StatusCode = StatusCodes.Status201Created;
            return Task.CompletedTask;
        });

        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        _responseBody.Length.Should().Be(0);
        _logger.Entries.Should().BeEmpty();
    }

    [Theory]
    [InlineData(StatusCodes.Status400BadRequest, "Invalid request.")]
    [InlineData(StatusCodes.Status401Unauthorized, "Invalid credentials.")]
    [InlineData(StatusCodes.Status403Forbidden, "Access denied.")]
    [InlineData(StatusCodes.Status404NotFound, "Resource not found.")]
    public async Task InvokeAsync_ReturnsStatusCodeFromMainHttpException(int statusCode, string message)
    {
        MainHttpException exception = statusCode switch
        {
            StatusCodes.Status400BadRequest => new BadRequestException(message),
            StatusCodes.Status401Unauthorized => new BadAuthenticationException(message),
            StatusCodes.Status403Forbidden => new ForbiddenException(message),
            _ => new NotFoundException(message)
        };

        await InvokeAsync(_ => throw exception);

        _httpContext.Response.StatusCode.Should().Be(statusCode);
        _httpContext.Response.ContentType.Should().Be("application/json");

        var body = ReadResponseBody();
        body.GetProperty("success").GetBoolean().Should().BeFalse();
        body.GetProperty("message").GetString().Should().Be(message);
        body.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsBadRequestWithErrors_WhenValidationExceptionIsThrown()
    {
        var validationException = new ValidationException(
        [
            new ValidationFailure("Email", "Email is required."),
            new ValidationFailure("Password", "Password is too short.")
        ]);

        await InvokeAsync(_ => throw validationException);

        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var body = ReadResponseBody();
        body.GetProperty("success").GetBoolean().Should().BeFalse();
        body.GetProperty("message").GetString().Should().Be("Validation failed");

        var errors = body.GetProperty("errors").EnumerateArray()
            .Select(x => x.GetString())
            .ToList();
        errors.Should().BeEquivalentTo("Email is required.", "Password is too short.");
    }

    [Fact]
    public async Task InvokeAsync_ReturnsUnauthorized_WhenNotActiveUserExceptionIsThrown()
    {
        await InvokeAsync(_ => throw new NotActiveUserException());

        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);

        var body = ReadResponseBody();
        body.GetProperty("success").GetBoolean().Should().BeFalse();
        body.GetProperty("message").GetString().Should().Be("User is not active");
    }

    [Fact]
    public async Task InvokeAsync_ReturnsInternalServerError_WhenUnexpectedExceptionIsThrown()
    {
        await InvokeAsync(_ => throw new InvalidOperationException("boom"));

        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        _httpContext.Response.ContentType.Should().Be("application/json");

        var body = ReadResponseBody();
        body.GetProperty("success").GetBoolean().Should().BeFalse();
        body.GetProperty("message").GetString()
            .Should().Be("An unexpected error occurred. Please try again later.");
        body.GetRawText().Should().NotContain("boom");
    }

    [Fact]
    public async Task InvokeAsync_LogsUnhandledException()
    {
        var exception = new InvalidOperationException("boom");

        await InvokeAsync(_ => throw exception);

        var entry = _logger.Entries.Should().ContainSingle().Which;
        entry.Level.Should().Be(LogLevel.Error);
        entry.Message.Should().Be("An unhandled exception has occurred.");
        entry.Exception.Should().BeSameAs(exception);
    }
}
