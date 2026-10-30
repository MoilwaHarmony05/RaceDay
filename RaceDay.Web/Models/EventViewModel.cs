namespace RaceDay.Web.Models;

public class EventViewModel
{
    public string? BannerImageUrl { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}