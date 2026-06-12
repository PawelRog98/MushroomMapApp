using System.Diagnostics;

namespace MushroomMapApp.Infrastructure.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await _next(context);
            sw.Stop();
            
            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds);
        }
        catch (Exception)
        {
            sw.Stop();
            _logger.LogError(
                "HTTP {Method} {Path} failed in {ElapsedMilliseconds}ms",
                context.Request.Method,
                context.Request.Path, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
