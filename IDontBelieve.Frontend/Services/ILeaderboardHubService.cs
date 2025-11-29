using IDontBelieve.Core.Models;

namespace IDontBelieve.Frontend.Services;

public interface ILeaderboardHubService
{
    event Action<LeaderboardUpdate>? OnLeaderboardUpdated;
    Task ConnectAsync();
    Task DisconnectAsync();
    Task<LeaderboardUpdate> GetLeaderboardAsync(int count = 10);
    Task SubscribeToUpdatesAsync();
    Task UnsubscribeFromUpdatesAsync();
    bool IsConnected { get; }
}