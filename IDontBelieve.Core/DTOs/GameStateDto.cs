using IDontBelieve.Core.Models;

namespace IDontBelieve.Core.DTOs;

public class GameStateDto
{
    public int CurrentPlayerId { get; set; }
    public int CardsInDeck { get; set; }
    public int CardsInDiscard { get; set; }
    public List<PlayerStateDto> Players { get; set; } = new();
    public GamePhase Phase { get; set; }
    public int GameBank { get; set; }
    public DateTime LastMoveAt { get; set; }
}

public class PlayerStateDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int CardCount { get; set; }
    public List<Card>? Hand { get; set; }   
    public PlayerStatus Status { get; set; }
    public int Position { get; set; }
}

public class UserStatsDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public decimal Coins { get; set; }
    public int GamesPlayed { get; set; }
    public int GamesWon { get; set; }
    public double WinRate { get; set; }
    public int GlobalRank { get; set; }
}