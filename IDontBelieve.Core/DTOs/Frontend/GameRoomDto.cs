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
    public int CurrentPlayerId { get; set; }
    public string Status { get; set; } = "Waiting"; // Enum string representation

    public int HostId { get; set; } = -1; 

    public GameRoomDto(){}

    [JsonConstructor]
    public GameRoomDto(int id, string name, int maxPlayers, int hostId,  List<PlayerDto> players, string status, int currentPlayerId)
    {
        Id = id;
        Name = name;
        MaxPlayers = maxPlayers;
        Players = players;
        HostId = hostId;
        Status = status;
        CurrentPlayerId = currentPlayerId;
    }

    public GameRoomDto(GameRoom room)
    {
        Id = room.Id;
        Name = room.Name;
        HostId = room.CreatedByUserId;
        MaxPlayers = room.MaxPlayers;
        
        Players = new List<PlayerDto>();
        foreach (var player in room.Players)
        {
            var isHost = player.UserId == room.CreatedByUserId;
            Players.Add(new PlayerDto(player.User, isHost));
        }

        Status = room.Status.ToString();
    }
}