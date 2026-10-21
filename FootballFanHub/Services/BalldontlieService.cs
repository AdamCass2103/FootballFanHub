using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace FootballFanHub.Services;

public interface IBalldontlieService
{
    Task<List<NBATeam>> GetTeamsAsync();
    Task<NBATeam?> GetTeamAsync(int teamId);
    Task<List<NBAGame>> GetGamesAsync(int teamId, int season);
    Task<List<NBARosterPlayer>> GetPlayersAsync(int teamId);
}

public class BalldontlieService : IBalldontlieService
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;

    public BalldontlieService(HttpClient http, IMemoryCache cache)
    {
        _http = http;
        _cache = cache;
    }

    // The team list rarely changes, so keep it for a day
    public async Task<List<NBATeam>> GetTeamsAsync()
    {
        return await GetAsync<NBATeam>("teams", TimeSpan.FromHours(24));
    }

    // Uses the cached team list, so this no longer costs an extra API call
    public async Task<NBATeam?> GetTeamAsync(int teamId)
    {
        return (await GetTeamsAsync()).FirstOrDefault(team => team.Id == teamId);
    }

    public async Task<List<NBAGame>> GetGamesAsync(int teamId, int season)
    {
        return await GetAsync<NBAGame>(
            $"games?team_ids[]={teamId}&seasons[]={season}&per_page=100", TimeSpan.FromMinutes(10));
    }

    // Free-tier endpoint: gives a team roster instead of the paid player-stats data
    public async Task<List<NBARosterPlayer>> GetPlayersAsync(int teamId)
    {
        return await GetAsync<NBARosterPlayer>(
            $"players?team_ids[]={teamId}&per_page=100", TimeSpan.FromHours(6));
    }

    private async Task<List<T>> GetAsync<T>(string endpoint, TimeSpan cacheFor)
    {
        // Same endpoint + same type = same cache entry, so repeat visits skip the API
        var cacheKey = $"nba:{typeof(T).Name}:{endpoint}";
        if (_cache.TryGetValue(cacheKey, out List<T>? cached) && cached is not null)
            return cached;

        // Base address and auth header are configured once in Program.cs
        var response = await _http.GetAsync(endpoint);

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            throw new Exception("API rate limit exceeded, please try again later.");
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new Exception("This NBA data isn't available on the free balldontlie plan.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var details = await response.Content.ReadAsStringAsync();
            throw new Exception($"NBA API returned {(int)response.StatusCode} ({response.ReasonPhrase}). {details}");
        }

        var result = await response.Content.ReadFromJsonAsync<BalldontlieResponse<T>>();
        var data = result?.Data ?? new List<T>();

        // Only successful responses reach this line, so an error is retried on the next visit
        _cache.Set(cacheKey, data, cacheFor);
        return data;
    }
}

public class BalldontlieResponse<T>
{
    public List<T> Data { get; set; } = new();
}

public class NBATeam
{
    public int Id { get; set; }
    public string Full_Name { get; set; } = "";
    public string Abbreviation { get; set; } = "";
    public string City { get; set; } = "";
    public string Name { get; set; } = "";
    public string Conference { get; set; } = "";
    public string Division { get; set; } = "";

    [JsonIgnore]
    public string LogoUrl =>
        $"https://a.espncdn.com/i/teamlogos/nba/500/{Abbreviation.ToLowerInvariant()}.png";
}

public class NBAGame
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public NBATeam Home_Team { get; set; } = new();
    public NBATeam Visitor_Team { get; set; } = new();
    public int Home_Team_Score { get; set; }
    public int Visitor_Team_Score { get; set; }
    public string Status { get; set; } = "";
}

public class NBARosterPlayer
{
    public int Id { get; set; }
    public string First_Name { get; set; } = "";
    public string Last_Name { get; set; } = "";
    public string? Position { get; set; }
    public string? Height { get; set; }
    public string? Weight { get; set; }
    public string? Jersey_Number { get; set; }
    public string? College { get; set; }
    public string? Country { get; set; }

    [JsonIgnore]
    public string FullName => $"{First_Name} {Last_Name}".Trim();
}