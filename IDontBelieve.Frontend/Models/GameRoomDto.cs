namespace IDontBelieve.Frontend.Models;

public class GameRoomDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MaxPlayers { get; set; }
    public int CurrentPlayerCount => Players?.Count ?? 0;
    public List<PlayerDto> Players { get; set; } = new();
    public string Status { get; set; } = "Waiting"; // Enum string representation
}