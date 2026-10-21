using System.Text.Json;

namespace FootballFanHub.Services;

public interface INbaFavouritesRepository
{
    Task<List<NBATeam>> GetAllAsync();
    Task<bool> IsFavouriteAsync(int teamId);
    Task<bool> AddAsync(NBATeam team);
    Task<bool> RemoveAsync(int teamId);
}

public class NbaFavouritesRepository : INbaFavouritesRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public NbaFavouritesRepository(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<List<NBATeam>> GetAllAsync()
    {
        await _lock.WaitAsync();
        try
        {
            return await ReadAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> IsFavouriteAsync(int teamId)
    {
        var teams = await GetAllAsync();
        return teams.Any(t => t.Id == teamId);
    }

    public async Task<bool> AddAsync(NBATeam team)
    {
        await _lock.WaitAsync();
        try
        {
            var teams = await ReadAsync();
            if (teams.Any(t => t.Id == team.Id))
                return false; // duplicate

            teams.Add(team);
            await WriteAsync(teams);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> RemoveAsync(int teamId)
    {
        await _lock.WaitAsync();
        try
        {
            var teams = await ReadAsync();
            var removed = teams.RemoveAll(t => t.Id == teamId) > 0;
            if (removed)
                await WriteAsync(teams);
            return removed;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<List<NBATeam>> ReadAsync()
    {
        if (!File.Exists(_filePath))
            return new List<NBATeam>();

        try
        {
            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<NBATeam>>(json, JsonOptions) ?? new List<NBATeam>();
        }
        catch (JsonException)
        {
            // Invalid JSON — treat as empty rather than crashing
            return new List<NBATeam>();
        }
    }

    private async Task WriteAsync(List<NBATeam> teams)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(teams, JsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }
}