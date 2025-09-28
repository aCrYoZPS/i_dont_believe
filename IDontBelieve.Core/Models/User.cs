using System.ComponentModel.DataAnnotations;
    
namespace IDontBelieve.Core.Models;

public class User
{   
    public int Id { get; set; }
    [Required, MaxLength(50)] public string UserName { get; set; } = string.Empty;
    [Required, MaxLength(255)] public string Email { get; set; } = string.Empty;
    public int Rating { get; set; } = 1000;
    public decimal Coins { get; set; } = 1000;
    public int GamesPlayed { get; set; } 
    public int GamesWon { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();
    public double WinRate => GamesPlayed > 0 ? (double)GamesWon / GamesPlayed : 0;
}