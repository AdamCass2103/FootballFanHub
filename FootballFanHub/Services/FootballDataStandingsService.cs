using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FootballFanHub.Exceptions;
using FootballFanHub.Models;

namespace FootballFanHub.Services;

public class FootballDataStandingsService : IStandingsService
{
    private readonly HttpClient _http;

    public FootballDataStandingsService(HttpClient http) => _http = http;

    public async Task<List<TeamStanding>> GetStandingsAsync(string competitionCode)
    {
        try
        {
            var response = await _http.GetAsync($"competitions/{competitionCode}/standings");

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                throw new ApiException("football-data.org rate limit reached. Try again in a minute.");
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                throw new ApiException("football-data.org rejected the API key, or this league isn't on the free plan.");
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<StandingsResponse>()
                       ?? throw new ApiException("Empty response from football-data.org.");

            var total = data.Standings.FirstOrDefault(s => s.Type == "TOTAL")
                        ?? throw new ApiException("No standings table found in the response.");

            var table = total.Table.Select(r => new TeamStanding
            {
                Position = r.Position,
                TeamName = r.Team.ShortName != "" ? r.Team.ShortName : r.Team.Name,
                CrestUrl = r.Team.Crest,
                Played = r.PlayedGames,
                Won = r.Won,
                Drawn = r.Draw,
                Lost = r.Lost,
                GoalDifference = r.GoalDifference,
                Points = r.Points
            }).ToList();

            table.Sort();   // uses TeamStanding.CompareTo
            return table;
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException("Couldn't reach football-data.org.", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ApiException("football-data.org took too long to respond.", ex);
        }
        catch (JsonException ex)
        {
            throw new ApiException("Unexpected data format from football-data.org.", ex);
        }
    }
}
