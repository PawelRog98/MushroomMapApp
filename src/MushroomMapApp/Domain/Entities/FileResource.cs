using System.ComponentModel.DataAnnotations.Schema;
using MushroomMapApp.Domain.Abstractions;
using MushroomMapApp.Domain.Enums;

namespace MushroomMapApp.Domain.Entities;

public class FileResource : ICommonData
{
    public FileResource()
    {
        Variant = new HashSet<FileResource>();
    }

    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public string FileName { get; set; }
    public string? Description { get; set; }
    public string? ContentType { get; set; }
    public long Size { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string Type  { get; set; }
    public long? LocationId { get; set; }
    public long? ParentFileResourceId { get; set; }
    public FileResource? ParentFileResource { get; set; }
    public long? UserId { get; set; }
    public ICollection<FileResource>? Variant { get; set; }

    [NotMapped]
    public FileType TypeEnum
    {
        get => Enum.TryParse(Type, true, out FileType fileType) ? fileType : FileType.Document;
        set => Type = value.ToString();
    }
}
