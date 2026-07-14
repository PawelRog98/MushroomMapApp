namespace MushroomMapApp.Features.Files.DownloadImage;

public class ImageResultDto
{
    public byte[] FileBytes { get; set; }
    public string ContentType { get; set; }
    public string FileName { get; set; }
}
