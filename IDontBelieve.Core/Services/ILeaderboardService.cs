using IDontBelieve.Core.Models;

namespace IDontBelieve.Core.Services;

public interface ILeaderboardService
{
    Task<LeaderboardUpdate> GetLeaderboardAsync(int count = 10);
    Task NotifyLeaderboardUpdatedAsync();
}