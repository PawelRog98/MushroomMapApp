namespace MushroomMapApp.Domain.Abstractions;

public interface ICommonData
{
    long Id { get; set; }
    Guid PublicId { get; set; }
}