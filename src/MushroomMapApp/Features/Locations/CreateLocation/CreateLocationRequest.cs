namespace MushroomMapApp.Features.Locations.CreateLocation;

public class CreateLocationRequest
{
    public string Name { get; set; }
    public string Text { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public IFormFileCollection Images { get; set; }
}
