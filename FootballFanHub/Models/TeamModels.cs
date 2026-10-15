namespace FootballFanHub.Models;

public class TeamSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ShortName { get; set; } = "";
    public string CrestUrl { get; set; } = "";
    public string Venue { get; set; } = "";
    public string League { get; set; } = "";
}

public class SquadMember
{
    public string Name { get; set; } = "";
    public string Position { get; set; } = "";
    public string Nationality { get; set; } = "";
    public int? Age { get; set; }
}

public class TeamDetail
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string CrestUrl { get; set; } = "";
    public string Venue { get; set; } = "";
    public int? Founded { get; set; }
    public string ClubColours { get; set; } = "";
    public string Website { get; set; } = "";
    public string Coach { get; set; } = "";
    public List<SquadMember> Squad { get; set; } = new();
}

public class MatchSummary
{
    public int Id { get; set; }
    public DateTime UtcDate { get; set; }
    public string Competition { get; set; } = "";
    public int HomeTeamId { get; set; }
    public string HomeTeamName { get; set; } = "";
    public int AwayTeamId { get; set; }
    public string AwayTeamName { get; set; } = "";
    public int? HomeGoals { get; set; }
    public int? AwayGoals { get; set; }
}