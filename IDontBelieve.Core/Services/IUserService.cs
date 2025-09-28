using IDontBelieve.Core.DTOs;
using IDontBelieve.Core.Models;

namespace IDontBelieve.Core.Services;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(string username, string email);
    Task<bool> UpdateUsernameAsync(int userId, string newName);
    Task UpdateUserStatsAsync(int userId, bool won, decimal coinsChange);
    Task<int> GetGlobalRankAsync(int userId);
    Task<List<UserStatsDto>> GetTopPlayersAsync(int count = 100);
    Task<bool> HasEnoughCoinsAsync(int userId, decimal amount);
    Task<UserStatsDto?> GetUserStatsAsync(int userId);
}