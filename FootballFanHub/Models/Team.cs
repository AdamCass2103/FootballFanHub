namespace FootballFanHub.Models;

public class Team
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? BadgeUrl { get; set; }
    public string? Stadium { get; set; }
    public string? Founded { get; set; }
    public string? League { get; set; }
}