using IDontBelieve.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IDontBelieve.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet("profile")]
    public async Task<ActionResult> GetProfile()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null) return NotFound();
        
        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.Rating,
            user.Coins,
            user.GamesPlayed,
            user.GamesWon,
            user.WinRate,
            user.CreatedAt
        });
    }

    [HttpGet("leaderboard")]
    [AllowAnonymous]
    public async Task<ActionResult> GetLeaderboard([FromQuery] int count = 10)
    {
        var topPlayers = await _userRepository.GetTopPlayersByRatingAsync(count);
        return Ok(topPlayers.Select(p => new
        {
            p.Id,
            p.UserName,
            p.Rating,
            p.GamesPlayed,
            p.GamesWon,
            p.WinRate
        }));
    }
}