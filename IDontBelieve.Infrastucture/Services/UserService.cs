using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IDontBelieve.Core.Models;
using IDontBelieve.Core.Services;
using IDontBelieve.Core.DTOs;
using IDontBelieve.Infrastructure.Data;
using IDontBelieve.Core.Game;

namespace IDontBelieve.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(ApplicationDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> CreateUserAsync(string username, string email)
    {
        var user = new User
        {
            UserName = username,
            Email = email,
            Rating = GameRules.DefaultRating,
            Coins = GameRules.DefaultCoins,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created new user: {Username} ({Email})", username, email);
        return user;
    }

    public async Task<bool> UpdateUsernameAsync(int userId, string newName)
    {
        if (await _context.Users.AnyAsync(u => u.UserName == newName && u.Id != userId))
        {
            _logger.LogWarning("Username {Username} is already taken", newName);
            return false;
        }

        var user = await GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", userId);
            return false;
        }

        var oldName = user.UserName;
        user.UserName = newName;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated username for user {UserId} from {OldName} to {NewName}", 
            userId, oldName, newName);
        return true;
    }

    public async Task UpdateUserStatsAsync(int userId, bool won, decimal coinsChange)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", userId);
            return;
        }

        user.GamesPlayed++;
        if (won) user.GamesWon++;
        user.Coins += coinsChange;
        user.UpdatedAt = DateTime.UtcNow;

        user.Rating = CalculateRating(user.GamesWon, user.GamesPlayed, won);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated stats for user {UserId}: Games={Games}, Won={Won}, Coins={Coins}, Rating={Rating}",
            userId, user.GamesPlayed, user.GamesWon, user.Coins, user.Rating);
    }

    public async Task<int> GetGlobalRankAsync(int userId)
    {
        var userRating = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Rating)
            .FirstOrDefaultAsync();

        if (userRating == 0) return -1;

        var rank = await _context.Users
            .CountAsync(u => u.Rating > userRating);

        return rank + 1;
    }

    public async Task<List<UserStatsDto>> GetTopPlayersAsync(int count = 100)
    {
        var topUsers = await _context.Users
            .OrderByDescending(u => u.Rating)
            .Take(count)
            .ToListAsync();

        var result = new List<UserStatsDto>();
        for (int i = 0; i < topUsers.Count; i++)
        {
            var user = topUsers[i];
            result.Add(new UserStatsDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Rating = user.Rating,
                Coins = user.Coins,
                GamesPlayed = user.GamesPlayed,
                GamesWon = user.GamesWon,
                WinRate = user.WinRate,
                GlobalRank = i + 1
            });
        }

        return result;
    }

    public async Task<bool> HasEnoughCoinsAsync(int userId, decimal amount)
    {
        var userCoins = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Coins)
            .FirstOrDefaultAsync();

        return userCoins >= amount;
    }

    public async Task<UserStatsDto?> GetUserStatsAsync(int userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null) return null;

        var globalRank = await GetGlobalRankAsync(userId);

        return new UserStatsDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Rating = user.Rating,
            Coins = user.Coins,
            GamesPlayed = user.GamesPlayed,
            GamesWon = user.GamesWon,
            WinRate = user.WinRate,
            GlobalRank = globalRank
        };
    }

    private int CalculateRating(int gamesWon, int gamesPlayed, bool lastGameWon)
    {
        if (gamesPlayed == 0) return GameRules.DefaultRating;

        var baseRating = GameRules.DefaultRating;
        var winRatio = (double)gamesWon / gamesPlayed;
        
        var calculatedRating = (int)(baseRating + (winRatio * 500) + (gamesPlayed * 2));
        
        if (lastGameWon) calculatedRating += GameRules.RatingWinBonus;
        else calculatedRating += GameRules.RatingLossBonus;

        return Math.Max(calculatedRating, 100); 
    }
}
