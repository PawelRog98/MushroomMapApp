using System.ComponentModel;

namespace MushroomMapApp.Shared.Response;

public class ErrorResponse : Response<object>
{
    [DefaultValue(false)]
    public new bool Success { get; set; } = false;

    public ErrorResponse()
    {
        Success = false;
    }
    
    public  ErrorResponse(string message) : base(message)
    {
        Success = false;
    }

    public ErrorResponse(string message, string[] errors) : base(message, errors)
    {
        Success = false;
    }

    public ErrorResponse(string message, string[] errors, dynamic metaData) : base(message, errors)
    {
        Success = false;
        MetaData = metaData;
    }

    public ErrorResponse(string message, dynamic metaData) : base(message)
    {
        Success = false;
        MetaData = metaData;
    }
}