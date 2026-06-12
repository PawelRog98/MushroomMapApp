using System.Net;
using System.Text.Json;
using FluentValidation;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Shared.Response;

namespace MushroomMapApp.Infrastructure.Middlewares;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        ErrorResponse response;

        if (exception is MainHttpException httpException)
        {
            context.Response.StatusCode = httpException.StatusCode;
            response = new ErrorResponse(httpException.Message);
        }
        else if (exception is ValidationException validationException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            response = new ErrorResponse("Validation failed", 
                validationException.Errors.Select(e => e.ErrorMessage).ToArray());
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response = new ErrorResponse("An unexpected error occurred. Please try again later.");
        }

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        return context.Response.WriteAsync(json);
    }
}
