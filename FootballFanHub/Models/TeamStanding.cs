namespace FootballFanHub.Models;

public class TeamStanding : IComparable<TeamStanding>
{
    public int Position { get; set; }
    public string TeamName { get; set; } = "";
    public string CrestUrl { get; set; } = "";
    public int Played { get; set; }
    public int Won { get; set; }
    public int Drawn { get; set; }
    public int Lost { get; set; }
    public int GoalDifference { get; set; }
    public int Points { get; set; }

    // Best team first: more points, then better goal difference
    public int CompareTo(TeamStanding? other)
    {
        if (other is null) return 1;
        int byPoints = other.Points.CompareTo(Points);
        return byPoints != 0 ? byPoints : other.GoalDifference.CompareTo(GoalDifference);
    }
}