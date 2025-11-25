using IDontBelieve.Core.Models;
using IDontBelieve.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace IDontBelieve.API.Hubs;

[Authorize]
public class LeaderboardHub : Hub
{
    private readonly ILeaderboardService _leaderboardService;

    public LeaderboardHub(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    public async Task<LeaderboardUpdate> GetLeaderboard(int count = 10)
    {
        return await _leaderboardService.GetLeaderboardAsync(count);
    }

    public async Task SubscribeToLeaderboard()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "LeaderboardSubscribers");
        var leaderboard = await _leaderboardService.GetLeaderboardAsync();
        await Clients.Caller.SendAsync("LeaderboardUpdated", leaderboard);
    }

    public async Task UnsubscribeFromLeaderboard()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "LeaderboardSubscribers");
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await UnsubscribeFromLeaderboard();
        await base.OnDisconnectedAsync(exception);
    }
}