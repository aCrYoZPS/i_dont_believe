using System.Text.Json.Serialization;
using IDontBelieve.Core.Models;

namespace IDontBelieve.Core.DTOs.Frontend;

public class GameRoomDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MaxPlayers { get; set; }
    [JsonIgnore]
    public int CurrentPlayerCount => Players?.Count ?? 0;
    public List<PlayerDto> Players { get; set; } = new();
    public string Status { get; set; } = "Waiting"; // Enum string representation

    public GameRoomDto(){}

    [JsonConstructor]
    public GameRoomDto(int id, string name, int maxPlayers,  List<PlayerDto> players, string status)
    {
        Id = id;
        Name = name;
        MaxPlayers = maxPlayers;
        Players = players;
        Status = status;
    }

    public GameRoomDto(GameRoom room)
    {
        Id = room.Id;
        Name = room.Name;
        MaxPlayers = room.MaxPlayers;
        
        Players = new List<PlayerDto>();
        foreach (var player in room.Players)
        {
            Players.Add(new PlayerDto(player.User));
        }

        Status = room.Status.ToString();
    }
}