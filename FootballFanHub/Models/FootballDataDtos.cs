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
    public int Id { get; set; }                       // new
}

// ---- new ----
public class TeamsResponse
{
    public List<TeamFullDto> Teams { get; set; } = new();
}

public class TeamFullDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ShortName { get; set; } = "";
    public string Crest { get; set; } = "";
    public string? Venue { get; set; }
    public int? Founded { get; set; }
    public string? ClubColors { get; set; }
    public string? Website { get; set; }
    public CoachDto? Coach { get; set; }
    public List<PlayerDto> Squad { get; set; } = new();
}

public class CoachDto { public string Name { get; set; } = ""; }

public class PlayerDto
{
    public string Name { get; set; } = "";
    public string? Position { get; set; }
    public string? Nationality { get; set; }
    public string? DateOfBirth { get; set; }
}

public class MatchesResponse
{
    public List<MatchDto> Matches { get; set; } = new();
}

public class MatchDto
{
    public int Id { get; set; }
    public DateTime UtcDate { get; set; }
    public string Status { get; set; } = "";
    public CompetitionDto? Competition { get; set; }
    public TeamDto HomeTeam { get; set; } = new();
    public TeamDto AwayTeam { get; set; } = new();
    public ScoreDto? Score { get; set; }
}

public class CompetitionDto { public string Name { get; set; } = ""; }
public class ScoreDto { public ScoreTimeDto? FullTime { get; set; } }
public class ScoreTimeDto { public int? Home { get; set; } public int? Away { get; set; } }