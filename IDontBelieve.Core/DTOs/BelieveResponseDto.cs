using IDontBelieve.Core.Models;

namespace IDontBelieve.Core.DTOs;

public class BelieveResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public BelieveResult? Result { get; set; }
    public GameStateDto? NewState { get; set; }
    public int ChallengerId { get; set; }
    public int ChallengedPlayerId { get; set; }
    public bool GameEnded { get; set; }
    public int? WinnerId { get; set; }
}