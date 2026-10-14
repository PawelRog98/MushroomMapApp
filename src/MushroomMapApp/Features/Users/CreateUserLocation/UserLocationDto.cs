namespace MushroomMapApp.Features.Users.CreateUserLocation;

public class UserLocationDto
{
    public Guid PublicId { get; set; }
    public string LocationName { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
}
