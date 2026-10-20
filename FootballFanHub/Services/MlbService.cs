using System.Text.Json;
using System.Text.Json.Serialization;

namespace FootballFanHub.Services;

public interface IMlbService
{
    Task<List<MlbTeam>> GetTeamsAsync(int season);
    Task<List<MlbRosterPlayer>> GetRosterAsync(string teamId);
    Task<List<MlbPlayer>> SearchPlayersAsync(string name);
    Task<MlbPlayer?> GetPlayerAsync(string playerId);
    Task<List<MlbPlayerTeam>> GetPlayerTeamsAsync(string playerId, int? season = null);
    Task<MlbHittingStats?> GetHittingStatsAsync(string playerId, int season);
    Task<MlbPitchingStats?> GetPitchingStatsAsync(string playerId, int season);
}

public sealed class MlbService : IMlbService
{
    private const string BaseUrl = "http://lookup-service-prod.mlb.com/json/named";
    private readonly HttpClient _http;

    public MlbService(HttpClient http)
    {
        _http = http;
        _http.Timeout = TimeSpan.FromSeconds(15);
    }

    public Task<List<MlbTeam>> GetTeamsAsync(int season) =>
        GetRowsAsync<MlbTeam>(
            $"team_all_season.bam?sport_code='mlb'&all_star_sw='N'&sort_order=name_asc&season='{season}'",
            "team_all_season");

    public Task<List<MlbRosterPlayer>> GetRosterAsync(string teamId) =>
        GetRowsAsync<MlbRosterPlayer>(
            $"roster_40.bam?team_id='{Uri.EscapeDataString(teamId)}'",
            "roster_40");

    public Task<List<MlbPlayer>> SearchPlayersAsync(string name)
    {
        var encodedName = Uri.EscapeDataString($"'{name.Trim()}%25'");
        return GetRowsAsync<MlbPlayer>(
            $"search_player_all.bam?sport_code='mlb'&active_sw='Y'&name_part={encodedName}",
            "search_player_all");
    }

    public async Task<MlbPlayer?> GetPlayerAsync(string playerId)
    {
        var rows = await GetRowsAsync<MlbPlayer>(
            $"player_info.bam?sport_code='mlb'&player_id='{Uri.EscapeDataString(playerId)}'",
            "player_info");
        return rows.FirstOrDefault();
    }

    public Task<List<MlbPlayerTeam>> GetPlayerTeamsAsync(string playerId, int? season = null)
    {
        var seasonQuery = season.HasValue ? $"&season='{season.Value}'" : "";
        return GetRowsAsync<MlbPlayerTeam>(
            $"player_teams.bam?player_id='{Uri.EscapeDataString(playerId)}'{seasonQuery}",
            "player_teams");
    }

    public async Task<MlbHittingStats?> GetHittingStatsAsync(string playerId, int season)
    {
        var rows = await GetRowsAsync<MlbHittingStats>(
            $"sport_hitting_tm.bam?league_list_id='mlb'&game_type='R'&season='{season}'&player_id='{Uri.EscapeDataString(playerId)}'",
            "sport_hitting_tm");
        return rows.FirstOrDefault();
    }

    public async Task<MlbPitchingStats?> GetPitchingStatsAsync(string playerId, int season)
    {
        var rows = await GetRowsAsync<MlbPitchingStats>(
            $"sport_pitching_tm.bam?league_list_id='mlb'&game_type='R'&season='{season}'&player_id='{Uri.EscapeDataString(playerId)}'",
            "sport_pitching_tm");
        return rows.FirstOrDefault();
    }

    private async Task<List<T>> GetRowsAsync<T>(string path, string endpointName)
    {
        using var response = await _http.GetAsync($"{BaseUrl}.{path}");
        if (!response.IsSuccessStatusCode)
        {
            var details = await response.Content.ReadAsStringAsync();
            throw new Exception($"MLB API returned {(int)response.StatusCode} ({response.ReasonPhrase}). {details}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        if (!document.RootElement.TryGetProperty(endpointName, out var endpoint)
            || !endpoint.TryGetProperty("queryResults", out var queryResults)
            || !queryResults.TryGetProperty("row", out var row))
        {
            return new List<T>();
        }

        if (row.ValueKind == JsonValueKind.Array)
        {
            return JsonSerializer.Deserialize<List<T>>(row.GetRawText(), JsonOptions) ?? new List<T>();
        }

        if (row.ValueKind == JsonValueKind.Object)
        {
            var item = JsonSerializer.Deserialize<T>(row.GetRawText(), JsonOptions);
            return item is null ? new List<T>() : new List<T> { item };
        }

        return new List<T>();
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}

public class MlbTeam
{
    public string Mlb_Org_Id { get; set; } = "";
    public string Franchise_Code { get; set; } = "";
    public string Name_Display_Full { get; set; } = "";
    public string Name_Display_Short { get; set; } = "";
    public string City { get; set; } = "";
    public string Address_City { get; set; } = "";
    public string Venue_Name { get; set; } = "";
    public string Venue_Id { get; set; } = "";
    public string League { get; set; } = "";
    public string League_Abbrev { get; set; } = "";
    public string Division { get; set; } = "";
    public string Phone_Number { get; set; } = "";
}

public class MlbRosterPlayer
{
    public string Player_Id { get; set; } = "";
    public string Name_Display_First_Last { get; set; } = "";
    public string Name_First { get; set; } = "";
    public string Name_Last { get; set; } = "";
    public string Position_Txt { get; set; } = "";
    public string Primary_Position { get; set; } = "";
    public string Jersey_Number { get; set; } = "";
    public string Bats { get; set; } = "";
    public string Throws { get; set; } = "";
    public string Birth_Date { get; set; } = "";

    [JsonIgnore]
    public string DisplayName => string.IsNullOrWhiteSpace(Name_Display_First_Last)
        ? $"{Name_First} {Name_Last}".Trim()
        : Name_Display_First_Last;
}

public class MlbPlayer
{
    public string Player_Id { get; set; } = "";
    public string Name_Display_First_Last { get; set; } = "";
    public string Name_Display_Last_First { get; set; } = "";
    public string Name_First { get; set; } = "";
    public string Name_Last { get; set; } = "";
    public string Name_Use { get; set; } = "";
    public string Team_Full { get; set; } = "";
    public string Team_Abbrev { get; set; } = "";
    public string Position { get; set; } = "";
    public string Primary_Position_Txt { get; set; } = "";
    public string Birth_Date { get; set; } = "";
    public string Birth_Country { get; set; } = "";
    public string Bats { get; set; } = "";
    public string Throws { get; set; } = "";
    public string Status { get; set; } = "";

    [JsonIgnore]
    public string DisplayName => string.IsNullOrWhiteSpace(Name_Display_First_Last)
        ? $"{Name_First} {Name_Last}".Trim()
        : Name_Display_First_Last;
}

public class MlbPlayerTeam
{
    public string Season { get; set; } = "";
    public string Org_Full { get; set; } = "";
    public string Team { get; set; } = "";
    public string Team_Abbrev { get; set; } = "";
    public string League_Full { get; set; } = "";
    public string Primary_Position { get; set; } = "";
    public string Status { get; set; } = "";
}

public class MlbHittingStats
{
    public string Season { get; set; } = "";
    public string Team_Full { get; set; } = "";
    public string G { get; set; } = "";
    public string AB { get; set; } = "";
    public string H { get; set; } = "";
    public string HR { get; set; } = "";
    public string RBI { get; set; } = "";
    public string AVG { get; set; } = "";
    public string OBP { get; set; } = "";
    public string SLG { get; set; } = "";
    public string OPS { get; set; } = "";
}

public class MlbPitchingStats
{
    public string Season { get; set; } = "";
    public string Team_Full { get; set; } = "";
    public string G { get; set; } = "";
    public string W { get; set; } = "";
    public string L { get; set; } = "";
    public string ERA { get; set; } = "";
    public string IP { get; set; } = "";
    public string SO { get; set; } = "";
    public string WHIP { get; set; } = "";
}
