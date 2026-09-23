namespace MushroomMapApp.Domain.Models;

public class EmailMessage
{
    public string To { get; set; }
    public string Subject { get; set; }
    public string? Body { get; set; }
    public string HtmlBody { get; set; }
}
