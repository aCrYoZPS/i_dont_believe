using IDontBelieve.Core.Models;

namespace IDontBelieve.Infrastructure.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<List<User>> GetTopPlayersByRatingAsync(int count);
    Task<int> GetUserRankAsync(int userId);
    Task<bool> IsUsernameAvailableAsync(string username, int? excludeUserId = null);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
}