using System.ComponentModel.DataAnnotations;

namespace IDontBelieve.Core.Models;

public class GameRoom
{
    public int Id { get; set; }
    
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Range(3, 6)] public int MaxPlayers { get; set; }
    
    public DeckType DeckType { get; set; }
    public bool ShowCardCount { get; set; }
    public GameRoomStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }
    
    public virtual ICollection<GamePlayer> Players { get; set; } = new List<GamePlayer>();
    public virtual GameState? GameState { get; set; }
    public virtual User CreatedBy { get; set; } = null!;
    
    public int CurrentPlayersCount => Players.Count;
    public bool IsFull => CurrentPlayersCount >= MaxPlayers;
    public bool CanStart => CurrentPlayersCount >= 2;
}