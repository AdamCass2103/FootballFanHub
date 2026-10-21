using System.Text.Json;
using FootballFanHub.Models;

namespace FootballFanHub.Services;

public class JsonFavouritesRepository : IFavouritesRepository
{
    private readonly string _path;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public JsonFavouritesRepository(string path) => _path = Path.GetFullPath(path);

    public async Task<List<FavouriteTeam>> GetAllAsync()
    {
        await _lock.WaitAsync();
        try { return await ReadAsync(); }
        finally { _lock.Release(); }
    }

    public async Task<bool> IsFavouriteAsync(int teamId)
    {
        var all = await GetAllAsync();
        return all.Any(t => t.Id == teamId);
    }

    public async Task<bool> AddAsync(FavouriteTeam team)
    {
        await _lock.WaitAsync();
        try
        {
            var all = await ReadAsync();
            if (all.Any(t => t.Id == team.Id)) return false;   // no duplicates

            all.Add(team);
            await WriteAsync(all);
            return true;
        }
        finally { _lock.Release(); }
    }

    public async Task<bool> RemoveAsync(int teamId)
    {
        await _lock.WaitAsync();
        try
        {
            var all = await ReadAsync();
            int removed = all.RemoveAll(t => t.Id == teamId);
            if (removed == 0) return false;

            await WriteAsync(all);
            return true;
        }
        finally { _lock.Release(); }
    }

    private async Task<List<FavouriteTeam>> ReadAsync()
    {
        if (!File.Exists(_path)) return new List<FavouriteTeam>();
        try
        {
            var json = await File.ReadAllTextAsync(_path);
            return JsonSerializer.Deserialize<List<FavouriteTeam>>(json) ?? new List<FavouriteTeam>();
        }
        catch (JsonException)
        {
            return new List<FavouriteTeam>();   
        }
    }

    private async Task WriteAsync(List<FavouriteTeam> teams)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var json = JsonSerializer.Serialize(teams, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_path, json);
    }
}