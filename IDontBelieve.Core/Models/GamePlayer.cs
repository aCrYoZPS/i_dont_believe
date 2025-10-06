namespace IDontBelieve.Core.Models;

public class GamePlayer
{
    public int Id { get; set; }
    public int GameRoomId { get; set; }
    public int UserId { get; set; }
    public int Position { get; set; }
    public string HandJson { get; set; } = "[]";
    public PlayerStatus Status { get; set; }

    public virtual GameRoom GameRoom { get; set; } = null!;
    public virtual User User { get; set; } = null!;

    public List<Card> Hand
    {
        get => System.Text.Json.JsonSerializer.Deserialize<List<Card>>(HandJson) ?? new List<Card>();
        set => HandJson = System.Text.Json.JsonSerializer.Serialize(value);
    }
}