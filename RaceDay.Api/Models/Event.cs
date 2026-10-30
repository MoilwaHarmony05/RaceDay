namespace RaceDay.Api.Models;

public class Event
{
    public string? BannerImageUrl { get; set; }
    public int EventId { get; set; }
    public int OrganiserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public DateTime RegistrationDeadline { get; set; }
    public decimal Distance { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public Organiser Organiser { get; set; } = null!;
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}