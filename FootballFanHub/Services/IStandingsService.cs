using FootballFanHub.Models;

namespace FootballFanHub.Services;

public interface IStandingsService
{
    Task<List<TeamStanding>> GetStandingsAsync(string competitionCode);
}