using MushroomMapApp.Domain.Abstractions;

namespace MushroomMapApp.Domain.Entities;

public class User : ICommonData
{
    public User()
    {
        Tokens = new HashSet<Token>();
        Locations = new HashSet<Location>();
    }

    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public string PublicNick { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsEmailConfirmed { get; set; }
    public string? AccountInfo { get; set; }
    public long RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAtUtc { get; set; }

    public ICollection<Token> Tokens { get; set; }
    public ICollection<Location> Locations { get; set; }
}
