using System.ComponentModel.DataAnnotations.Schema;
using MushroomMapApp.Domain.Abstractions;
using MushroomMapApp.Domain.Enums;

namespace MushroomMapApp.Domain.Entities;

public class Token : ICommonData
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public string TokenData { get; set; } = string.Empty;
    public DateTime ExpireDateTime { get; set; }
    public string TokenTypeValue { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    
    [NotMapped]
    public TokenType TokenType
    {
        get => Enum.TryParse<TokenType>(TokenTypeValue, out var result) ? result : default;
        set => TokenTypeValue = value.ToString();
    }
}
