using FootballFanHub.Models;

namespace FootballFanHub.Services;

public interface IFavouritesRepository
{
    Task<List<FavouriteTeam>> GetAllAsync();
    Task<bool> IsFavouriteAsync(int teamId);
    Task<bool> AddAsync(FavouriteTeam team);    // false if it's already followed
    Task<bool> RemoveAsync(int teamId);         // false if it wasn't followed
}