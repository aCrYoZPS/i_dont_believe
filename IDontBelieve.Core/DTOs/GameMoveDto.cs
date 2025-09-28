using IDontBelieve.Core.Models;

namespace IDontBelieve.Core.DTOs;

public class GameMoveDto
{
    public List<Card> Cards { get; set; } = new();
    public CardRank ClaimedRank { get; set; }
    public int TargetPlayerId { get; set; }
}

public class GameMoveResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public GameStateDto? NewState { get; set; }
    public bool GameEnded { get; set; }
    public int? WinnerId { get; set; }
}