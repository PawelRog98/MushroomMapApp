using MushroomMapApp.Domain.Abstractions;
using Point = NetTopologySuite.Geometries.Point;

namespace MushroomMapApp.Domain.Entities;

public class UserSavedLocation : ICommonData
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public string LocationName { get; set; }
    public Point Coordinates { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
