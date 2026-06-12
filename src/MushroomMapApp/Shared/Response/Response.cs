using System.ComponentModel;

namespace MushroomMapApp.Shared.Response;

public class Response<T>
{
    public T? Data { get; set; }
        
    [DefaultValue(true)]
    public bool Success { get; set; }
    public string[] Errors { get; set; } = null;
    public dynamic? MetaData { get; set; } = null;
    public string Message { get; set; } = string.Empty;

    public Response()
    {
    }

    public Response(T data)
    {
        Data = data;
    }
    public Response(T data, bool success, string[] errors, dynamic metaData, string message) : this(data)
    {
        Success = success;
        Errors = errors;
        MetaData = metaData;
        Message = message;
    }

    public Response(string message, string[] errors)
    {
        Message = message;
        Errors = errors;
    }
}