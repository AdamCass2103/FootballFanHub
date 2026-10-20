using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FootballFanHub.Services;

public interface IBalldontlieService
{
    Task<List<NBATeam>> GetTeamsAsync();
    Task<NBATeam?> GetTeamAsync(int teamId);
    Task<List<NBAStanding>> GetStandingsAsync(int season);
    Task<List<NBAGame>> GetGamesAsync(int teamId, int season);
    Task<List<NBAPlayerStat>> GetPlayerStatsAsync(int teamId, int season);
}

public class BalldontlieService : IBalldontlieService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public BalldontlieService(HttpClient http, IConfiguration config)
    {
        _http = http;
        // Read key from configuration, never hardcode
        _apiKey = config["Balldontlie:ApiKey"] ?? throw new InvalidOperationException("API Key not configured");
    }

    public async Task<List<NBATeam>> GetTeamsAsync()
    {
        return await GetAsync<NBATeam>("teams");
    }

    public async Task<NBATeam?> GetTeamAsync(int teamId)
    {
        return (await GetTeamsAsync()).FirstOrDefault(team => team.Id == teamId);
    }

    public async Task<List<NBAStanding>> GetStandingsAsync(int season)
    {
        return await GetAsync<NBAStanding>($"standings?season={season}");
    }

    public async Task<List<NBAGame>> GetGamesAsync(int teamId, int season)
    {
        return await GetAsync<NBAGame>($"games?team_ids[]={teamId}&seasons[]={season}&per_page=100");
    }

    public async Task<List<NBAPlayerStat>> GetPlayerStatsAsync(int teamId, int season)
    {
        return await GetAsync<NBAPlayerStat>($"stats?team_ids[]={teamId}&seasons[]={season}&per_page=100");
    }

    private async Task<List<T>> GetAsync<T>(string endpoint)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://api.balldontlie.io/nba/v1/{endpoint}");
        request.Headers.Add("Authorization", _apiKey);

        var response = await _http.SendAsync(request);
        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            throw new Exception("API rate limit exceeded, please try again later.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var details = await response.Content.ReadAsStringAsync();
            throw new Exception($"NBA API returned {(int)response.StatusCode} ({response.ReasonPhrase}). {details}");
        }

        var result = await response.Content.ReadFromJsonAsync<BalldontlieResponse<T>>();
        return result?.Data ?? new List<T>();
    }
}

// Data models matching the API JSON structure
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

public class NBAStanding
{
    public NBATeam Team { get; set; } = new();
    public int Rank { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
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

public class NBAPlayerStat
{
    public NBAPlayer Player { get; set; } = new();
    public int Games_Played { get; set; }
    public double Pts { get; set; }
    public double Reb { get; set; }
    public double Ast { get; set; }
    public double Stl { get; set; }
    public double Blk { get; set; }
    public double Fg_Pct { get; set; }
}

public class NBAPlayer
{
    public int Id { get; set; }
    public string First_Name { get; set; } = "";
    public string Last_Name { get; set; } = "";

    [JsonIgnore]
    public string FullName => $"{First_Name} {Last_Name}".Trim();
}
