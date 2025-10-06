namespace IDontBelieve.Core.Models;

public class GameMove
{
    public int Id { get; set; }
    public int GameStateId { get; set; }
    public int MoveNumber { get; set; }
    public int PlayerId { get; set; }
    public int TargetPlayerId { get; set; }
    public string CardsPlayedJson { get; set; } = "[]";
    public MoveOutcome Outcome { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual GameState GameState { get; set; } = null!;
    
    public List<Card> CardsPlayed
    {
        get => System.Text.Json.JsonSerializer.Deserialize<List<Card>>(CardsPlayedJson) ?? new List<Card>();
        set => CardsPlayedJson = System.Text.Json.JsonSerializer.Serialize(value);
    }
}