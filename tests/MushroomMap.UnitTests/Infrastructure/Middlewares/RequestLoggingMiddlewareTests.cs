using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MushroomMap.UnitTests.Common.Logging;
using MushroomMapApp.Infrastructure.Middlewares;
using Xunit;

namespace MushroomMap.UnitTests.Infrastructure.Middlewares;

public class RequestLoggingMiddlewareTests
{
    private readonly RecordingLogger<RequestLoggingMiddleware> _logger = new();
    private readonly DefaultHttpContext _httpContext;

    public RequestLoggingMiddlewareTests()
    {
        _httpContext = new DefaultHttpContext();
        _httpContext.Request.Method = HttpMethods.Get;
        _httpContext.Request.Path = "/api/locations";
        _httpContext.Response.StatusCode = StatusCodes.Status200OK;
    }

    private Task InvokeAsync(RequestDelegate next)
        => new RequestLoggingMiddleware(next, _logger).InvokeAsync(_httpContext);

    [Fact]
    public async Task InvokeAsync_LogsRequestDuration_WhenNextSucceeds()
    {
        await InvokeAsync(_ => Task.CompletedTask);

        var entry = _logger.Entries.Should().ContainSingle().Which;
        entry.Level.Should().Be(LogLevel.Information);
        entry.Exception.Should().BeNull();
        entry.Message.Should().Contain("GET");
        entry.Message.Should().Contain("/api/locations");
        entry.Message.Should().Contain("200");
        entry.Message.Should().Contain("ms");
    }

    [Fact]
    public async Task InvokeAsync_LogsFailureAndRethrows_WhenNextThrows()
    {
        var exception = new InvalidOperationException("boom");

        var act = () => InvokeAsync(_ => throw exception);

        var thrown = await act.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Should().BeSameAs(exception);

        var entry = _logger.Entries.Should().ContainSingle().Which;
        entry.Level.Should().Be(LogLevel.Error);
        entry.Message.Should().Contain("GET");
        entry.Message.Should().Contain("/api/locations");
        entry.Message.Should().Contain("ms");
    }
}
