namespace FootballFanHub.Models;
public class Standing : IComparable<Standing>
{
    public int Rank { get; set; }
    public string TeamId { get; set; } = "";
    public string TeamName { get; set; } = "";
    public string? BadgeUrl { get; set; }
    public int Played { get; set; }
    public int Won { get; set; }
    public int Drawn { get; set; }
    public int Lost { get; set; }
    public int Points { get; set; }
    public int GoalDifference { get; set; }

    // Sorts by points DESC, then goal difference DESC
    public int CompareTo(Standing? other)
    {
        if (other is null) return -1;
        int byPoints = other.Points.CompareTo(Points);
        if (byPoints != 0) return byPoints;
        return other.GoalDifference.CompareTo(GoalDifference);
    }
}