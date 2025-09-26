namespace IDontBelieve.Core.Models;

public class GameState
{
    public int Id { get; set; }
    public int GameRoomId { get; set; }
    public int CurrentPlayerId { get; set; }
    public string DeckJson { get; set; } = "[]";
    public string DiscardPileJson { get; set; } = "[]";
    public int GameBank { get; set; }
    public GamePhase Phase { get; set; }
    public DateTime LastMoveAt { get; set; }

    public virtual GameRoom GameRoom { get; set; } = null!;
    public virtual ICollection<GameMove> MoveHistory { get; set; } = new List<GameMove>();
    
    
    public List<Card> Deck
    {
        get => System.Text.Json.JsonSerializer.Deserialize<List<Card>>(DeckJson) ?? new List<Card>();
        set => DeckJson = System.Text.Json.JsonSerializer.Serialize(value);
    }
    
    public List<Card> DiscardPile
    {
        get => System.Text.Json.JsonSerializer.Deserialize<List<Card>>(DiscardPileJson) ?? new List<Card>();
        set => DiscardPileJson = System.Text.Json.JsonSerializer.Serialize(value);
    }
}