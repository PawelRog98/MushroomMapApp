using MushroomMapApp.Domain.Abstractions;
using NetTopologySuite.Geometries;

namespace MushroomMapApp.Domain.Entities;

public class Location : ICommonData
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public string Name { get; set; }
    public string Text { get; set; }
    public Point Coordinates { get; set; }
    public long CreatedById { get; set; }
    public User CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
