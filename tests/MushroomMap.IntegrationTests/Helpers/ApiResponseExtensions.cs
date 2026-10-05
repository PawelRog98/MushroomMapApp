using System.Text.Json;
using MushroomMapApp.Shared.Response;

namespace MushroomMap.IntegrationTests.Helpers;

public static class ApiResponseExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static async Task<JsonElement> ReadBodyAsync(this HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(json))
            return default;

        return JsonSerializer.Deserialize<JsonElement>(json);
    }

    public static async Task<JsonElement> ReadApiPayloadAsync(this HttpResponseMessage response)
    {
        var body = await response.ReadBodyAsync();
        return body.ValueKind == JsonValueKind.Object && body.TryGetProperty("value", out var value)
            ? value
            : body;
    }

    public static async Task<Response<T>> ReadApiAsync<T>(this HttpResponseMessage response)
    {
        var payload = await response.ReadApiPayloadAsync();
        return payload.Deserialize<Response<T>>(SerializerOptions) ?? new Response<T>();
    }

    public static async Task<int> ReadApiStatusCodeAsync(this HttpResponseMessage response)
    {
        var body = await response.ReadBodyAsync();
        return body.ValueKind == JsonValueKind.Object
                   && body.TryGetProperty("statusCode", out var statusCode)
                   && statusCode.ValueKind == JsonValueKind.Number
            ? statusCode.GetInt32()
            : (int)response.StatusCode;
    }

    public static T? ToObject<T>(this JsonElement element)
        => element.Deserialize<T>(SerializerOptions);
}
