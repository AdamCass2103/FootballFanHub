namespace FootballFanHub.Models;

public class StandingsResponse
{
    public List<StandingGroup> Standings { get; set; } = new();
}

public class StandingGroup
{
    public string Type { get; set; } = "";          // "TOTAL", "HOME" or "AWAY"
    public List<TableRow> Table { get; set; } = new();
}

public class TableRow
{
    public int Position { get; set; }
    public TeamDto Team { get; set; } = new();
    public int PlayedGames { get; set; }
    public int Won { get; set; }
    public int Draw { get; set; }
    public int Lost { get; set; }
    public int Points { get; set; }
    public int GoalDifference { get; set; }
}

public class TeamDto
{
    public string Name { get; set; } = "";
    public string ShortName { get; set; } = "";
    public string Crest { get; set; } = "";
}