using System.Net.Http.Json;
using System.Text.Json;

namespace YourProjectName.Services;

public interface IBalldontlieService
{
    Task<List<NBATeam>> GetTeamsAsync();
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
        // Note: base URL for balldontlie is https://api.balldontlie.io
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.balldontlie.io/nba/v1/teams");
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _http.SendAsync(request);

        // Key: handle 429 rate limiting (free tier 5 times/minute)
        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            throw new Exception("API rate limit exceeded, please try again later.");
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<BalldontlieResponse<NBATeam>>();
        return result?.Data ?? new List<NBATeam>();
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
}