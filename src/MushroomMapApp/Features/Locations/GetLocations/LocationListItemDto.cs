namespace MushroomMapApp.Features.Locations.GetLocations;

public class LocationListItemDto
{
    public Guid PublicId { get; set; }
    public string AuthorName { get; set; }
    public Guid AuthorPublicId { get; set; }
    public string Name { get; set; }
    public string Text { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public List<ImageDto> Images { get; set; }
}

public class ImageDto
{
    public Guid PublicId { get; set; }
    public string Url  { get; set; }
    public string ThumbnailUrl { get; set; }
    public string ContentType { get; set; }
}
