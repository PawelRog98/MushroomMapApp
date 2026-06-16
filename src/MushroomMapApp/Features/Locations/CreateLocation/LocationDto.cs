namespace MushroomMapApp.Features.Locations.CreateLocation;

public class LocationDto
{
    public Guid PublicId { get; set; }
    public string Name { get; set; }
    public string Text { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
}
