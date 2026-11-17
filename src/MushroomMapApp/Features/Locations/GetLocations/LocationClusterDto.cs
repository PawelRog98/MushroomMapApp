namespace MushroomMapApp.Features.Locations.GetLocations;

public class LocationClusterDto
{
    public double Lat { get; set; }
    public double Lng { get; set; }
    public int Count { get; set; }

    public double South  { get; set; }
    public double West { get; set; }
    public double North { get; set; }
    public double East { get; set; }

    public int Zoom  { get; set; }
    public long CellX { get; set; }
    public long CellY { get; set; }
}
