using IDontBelieve.API.Hubs;
using IDontBelieve.Core.Models;
using IDontBelieve.Core.Services;
using IDontBelieve.Infrastructure.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace IDontBelieve.Infrastructure.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly IUserRepository _userRepository;
    private readonly IHubContext<LeaderboardHub> _hubContext;

    public LeaderboardService(IUserRepository userRepository, IHubContext<LeaderboardHub> hubContext)
    {
        _userRepository = userRepository;
        _hubContext = hubContext;
    }

    public async Task<LeaderboardUpdate> GetLeaderboardAsync(int count = 10)
    {
        var topUsers = await _userRepository.GetTopPlayersByRatingAsync(count);
        var leaderboardUsers = new List<LeaderboardUser>();

        for (int i = 0; i < topUsers.Count; i++)
        {
            var user = topUsers[i];
            leaderboardUsers.Add(new LeaderboardUser
            {
                Id = user.Id,
                UserName = user.UserName,
                Rating = user.Rating,
                GamesPlayed = user.GamesPlayed,
                GamesWon = user.GamesWon,
                WinRate = user.WinRate,
                Rank = i + 1
            });
        }

        return new LeaderboardUpdate
        {
            Users = leaderboardUsers,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public async Task NotifyLeaderboardUpdatedAsync()
    {
        var leaderboard = await GetLeaderboardAsync();
        await _hubContext.Clients.All.SendAsync("LeaderboardUpdated", leaderboard);
    }
}