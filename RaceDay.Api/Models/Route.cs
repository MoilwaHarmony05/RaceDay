namespace RaceDay.Api.Models;

public class Route
{
    public int RouteId { get; set; }
    public int CategoryId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public decimal Elevation { get; set; }
    public string? MapLink { get; set; }

    public Category Category { get; set; } = null!;
}