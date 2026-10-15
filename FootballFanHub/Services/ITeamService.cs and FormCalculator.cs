using FootballFanHub.Models;

namespace FootballFanHub.Services;

public interface ITeamService
{
    Task<List<TeamSummary>> GetAllTeamsAsync();
    Task<TeamDetail> GetTeamAsync(int teamId);
    Task<List<MatchSummary>> GetUpcomingMatchesAsync(int teamId, int count = 5);
    Task<List<MatchSummary>> GetRecentResultsAsync(int teamId, int count = 5);
}

// Pure logic with no API calls, so it's easy to unit test
public static class FormCalculator
{
    // 'W', 'D' or 'L' from this team's point of view, '?' if there's no score
    public static char ResultFor(MatchSummary match, int teamId)
    {
        if (match.HomeGoals is null || match.AwayGoals is null) return '?';

        bool isHome = match.HomeTeamId == teamId;
        int mine = isHome ? match.HomeGoals.Value : match.AwayGoals.Value;
        int theirs = isHome ? match.AwayGoals.Value : match.HomeGoals.Value;

        return mine > theirs ? 'W' : mine == theirs ? 'D' : 'L';
    }

    // Most recent result first
    public static List<char> GetForm(IEnumerable<MatchSummary> matches, int teamId, int count = 5) =>
        matches.OrderByDescending(m => m.UtcDate)
               .Take(count)
               .Select(m => ResultFor(m, teamId))
               .ToList();
}