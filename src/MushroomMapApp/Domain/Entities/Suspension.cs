using MushroomMapApp.Domain.Abstractions;
using MushroomMapApp.Domain.Enums;

namespace MushroomMapApp.Domain.Entities;

public class Suspension : ICommonData
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Reason { get; set; }
    public SuspensionStatusEnum Status { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public long SuspendedById { get; set; }
    public User SuspendedBy { get; set; }
}
