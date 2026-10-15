using FootballFanHub.Models;
using Microsoft.Extensions.Caching.Memory;

namespace FootballFanHub.Services;

public class FootballDataTeamService : ITeamService
{
    // League code -> display name
    private static readonly Dictionary<string, string> Leagues = new()
    {
        ["PL"] = "Premier League",
        ["PD"] = "La Liga",
        ["BL1"] = "Bundesliga",
        ["SA"] = "Serie A",
        ["FL1"] = "Ligue 1"
    };

    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;

    public FootballDataTeamService(HttpClient http, IMemoryCache cache)
    {
        _http = http;
        _cache = cache;
    }

    // No search endpoint exists, so load every club once, cache it, and filter in our own code
    public async Task<List<TeamSummary>> GetAllTeamsAsync()
    {
        const string key = "all-teams";
        if (_cache.TryGetValue(key, out List<TeamSummary>? cached) && cached is not null)
            return cached;

        var teams = new List<TeamSummary>();
        foreach (var (code, leagueName) in Leagues)
        {
            var data = await _http.GetFromFootballDataAsync<TeamsResponse>($"competitions/{code}/teams");
            teams.AddRange(data.Teams.Select(t => new TeamSummary
            {
                Id = t.Id,
                Name = t.Name,
                ShortName = string.IsNullOrEmpty(t.ShortName) ? t.Name : t.ShortName,
                CrestUrl = t.Crest,
                Venue = t.Venue ?? "",
                League = leagueName
            }));
        }

        teams.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        _cache.Set(key, teams, TimeSpan.FromHours(24));
        return teams;
    }

    public async Task<TeamDetail> GetTeamAsync(int teamId)
    {
        var key = $"team-{teamId}";
        if (_cache.TryGetValue(key, out TeamDetail? cached) && cached is not null)
            return cached;

        var t = await _http.GetFromFootballDataAsync<TeamFullDto>($"teams/{teamId}");
        var detail = new TeamDetail
        {
            Id = t.Id,
            Name = t.Name,
            CrestUrl = t.Crest,
            Venue = t.Venue ?? "",
            Founded = t.Founded,
            ClubColours = t.ClubColors ?? "",
            Website = t.Website ?? "",
            Coach = t.Coach?.Name ?? "",
            Squad = t.Squad.Select(p => new SquadMember
            {
                Name = p.Name,
                Position = p.Position ?? "Unknown",
                Nationality = p.Nationality ?? "",
                Age = AgeFrom(p.DateOfBirth)
            }).ToList()
        };

        _cache.Set(key, detail, TimeSpan.FromMinutes(30));
        return detail;
    }

    public async Task<List<MatchSummary>> GetUpcomingMatchesAsync(int teamId, int count = 5)
    {
        var matches = await GetMatchesAsync(
            $"teams/{teamId}/matches?status=SCHEDULED&limit={count}", $"upcoming-{teamId}");
        return matches.OrderBy(m => m.UtcDate).Take(count).ToList();
    }

    public async Task<List<MatchSummary>> GetRecentResultsAsync(int teamId, int count = 5)
    {
        // Ask for a recent window and pick the newest ourselves, so we never depend on the API's ordering.
        // The docs say dateTo is excluded, so we use tomorrow to include today's matches.
        var from = DateTime.UtcNow.AddDays(-60).ToString("yyyy-MM-dd");
        var to = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");

        var matches = await GetMatchesAsync(
            $"teams/{teamId}/matches?status=FINISHED&dateFrom={from}&dateTo={to}&limit=20", $"recent-{teamId}");
        return matches.OrderByDescending(m => m.UtcDate).Take(count).ToList();
    }

    private async Task<List<MatchSummary>> GetMatchesAsync(string path, string cacheKey)
    {
        if (_cache.TryGetValue(cacheKey, out List<MatchSummary>? cached) && cached is not null)
            return cached;

        var data = await _http.GetFromFootballDataAsync<MatchesResponse>(path);
        var result = data.Matches.Select(m => new MatchSummary
        {
            Id = m.Id,
            UtcDate = m.UtcDate,
            Competition = m.Competition?.Name ?? "",
            HomeTeamId = m.HomeTeam.Id,
            HomeTeamName = ShortName(m.HomeTeam),
            AwayTeamId = m.AwayTeam.Id,
            AwayTeamName = ShortName(m.AwayTeam),
            HomeGoals = m.Score?.FullTime?.Home,
            AwayGoals = m.Score?.FullTime?.Away
        }).ToList();

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
        return result;
    }

    private static string ShortName(TeamDto t) => string.IsNullOrEmpty(t.ShortName) ? t.Name : t.ShortName;

    private static int? AgeFrom(string? dateOfBirth)
    {
        if (!DateOnly.TryParse(dateOfBirth, out var dob)) return null;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        int age = today.Year - dob.Year;
        if (dob > today.AddYears(-age)) age--;
        return age;
    }
}