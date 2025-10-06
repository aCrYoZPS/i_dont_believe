using IDontBelieve.Core.Models;

namespace IDontBelieve.Core.DTOs;

public class RoomFilterDto
{   
    public string? RoomName { get; set; }
    public DeckType? DeckType { get; set; }
    public int? MaxPlayers { get; set; }
    public bool? ShowOnlyJoinable { get; set; } = true;
}

public class JoinRoomResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public GameRoom? Room { get; set; }
}