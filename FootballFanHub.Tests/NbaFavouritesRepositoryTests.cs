using FootballFanHub.Services;
using Xunit;

namespace FootballFanHub.Tests;

public sealed class NbaFavouritesRepositoryTests : IDisposable
{
    private readonly string _directory;
    private readonly string _filePath;
    private readonly NbaFavouritesRepository _repository;

    public NbaFavouritesRepositoryTests()
    {
        _directory = Path.Combine(Path.GetTempPath(), "FootballFanHubNbaTests", Guid.NewGuid().ToString());
        _filePath = Path.Combine(_directory, "nba-favourites.json");
        _repository = new NbaFavouritesRepository(_filePath);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenFileDoesNotExist()
    {
        var favourites = await _repository.GetAllAsync();

        Assert.Empty(favourites);
    }

    [Fact]
    public async Task AddAsync_PersistsTeam_AndIsFavouriteReturnsTrue()
    {
        var team = CreateTeam(1);

        var added = await _repository.AddAsync(team);
        var favourites = await _repository.GetAllAsync();
        var isFavourite = await _repository.IsFavouriteAsync(team.Id);

        Assert.True(added);
        Assert.True(isFavourite);
        var savedTeam = Assert.Single(favourites);
        Assert.Equal(team.Id, savedTeam.Id);
        Assert.Equal(team.Full_Name, savedTeam.Full_Name);
    }

    [Fact]
    public async Task AddAsync_ReturnsFalse_AndDoesNotCreateDuplicate()
    {
        var team = CreateTeam(1);

        await _repository.AddAsync(team);
        var addedAgain = await _repository.AddAsync(team);
        var favourites = await _repository.GetAllAsync();

        Assert.False(addedAgain);
        Assert.Single(favourites);
    }

    [Fact]
    public async Task RemoveAsync_RemovesExistingTeam_AndReturnsFalseForMissingTeam()
    {
        await _repository.AddAsync(CreateTeam(1));

        var removed = await _repository.RemoveAsync(1);
        var removedAgain = await _repository.RemoveAsync(1);

        Assert.True(removed);
        Assert.False(removedAgain);
        Assert.Empty(await _repository.GetAllAsync());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenJsonFileIsMalformed()
    {
        Directory.CreateDirectory(_directory);
        await File.WriteAllTextAsync(_filePath, "not valid json");

        var favourites = await _repository.GetAllAsync();

        Assert.Empty(favourites);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private static NBATeam CreateTeam(int id) => new()
    {
        Id = id,
        Full_Name = $"Test Team {id}",
        Abbreviation = $"T{id}",
        City = "Test City",
        Conference = "Test Conference",
        Division = "Test Division"
    };
}