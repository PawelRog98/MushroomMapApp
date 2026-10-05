using System.Globalization;
using System.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MushroomMap.IntegrationTests.Helpers;

public static class TestImages
{
    public const string PngContentType = "image/png";

    public static byte[] CreatePng(int width = 64, int height = 64)
    {
        using var image = new Image<Rgba32>(width, height, new Rgba32(30, 120, 60));
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }

    public static ByteArrayContent CreatePngContent(string fileName = "image.png")
    {
        var content = new ByteArrayContent(CreatePng());
        content.Headers.ContentType = new MediaTypeHeaderValue(PngContentType);
        return content;
    }

    public static MultipartFormDataContent CreateLocationForm(
        string name,
        string text,
        double lat = 50.0614,
        double lng = 19.9366,
        bool includeImage = true)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(name), "Name" },
            { new StringContent(text), "Text" },
            { new StringContent(lat.ToString(CultureInfo.InvariantCulture)), "Lat" },
            { new StringContent(lng.ToString(CultureInfo.InvariantCulture)), "Lng" }
        };

        if (includeImage)
            form.Add(CreatePngContent("location.png"), "Images", "location.png");

        return form;
    }

    public static MultipartFormDataContent CreateUpdateLocationForm(
        string name,
        string text,
        IEnumerable<Guid>? keepImageIds = null)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(name), "Name" },
            { new StringContent(text), "Text" }
        };

        foreach (var imageId in keepImageIds ?? [])
            form.Add(new StringContent(imageId.ToString()), "KeepImageIds");

        return form;
    }

    public static MultipartFormDataContent CreateAvatarForm(string contentType = PngContentType)
    {
        var content = new ByteArrayContent(CreatePng(80, 60));
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        return new MultipartFormDataContent
        {
            { content, "Image", "avatar.png" }
        };
    }
}
