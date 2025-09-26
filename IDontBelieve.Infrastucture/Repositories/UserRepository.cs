using Microsoft.EntityFrameworkCore;
using IDontBelieve.Core.Models;
using IDontBelieve.Infrastructure.Data;

namespace IDontBelieve.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<User>> GetTopPlayersByRatingAsync(int count)
    {
        return await _dbSet
            .OrderByDescending(u => u.Rating)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> GetUserRankAsync(int userId)
    {
        var userRating = await _dbSet
            .Where(u => u.Id == userId)
            .Select(u => u.Rating)
            .FirstOrDefaultAsync();

        if (userRating == 0) return -1;

        var rank = await _dbSet
            .CountAsync(u => u.Rating > userRating);

        return rank + 1;
    }

    public async Task<bool> IsUsernameAvailableAsync(string username, int? excludeUserId = null)
    {
        var query = _dbSet.Where(u => u.UserName == username);
        
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }
        
        return !await query.AnyAsync();
    }
}