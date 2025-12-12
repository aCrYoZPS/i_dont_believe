using System.Text.Json.Serialization;
using IDontBelieve.Core.Models;

namespace IDontBelieve.Core.DTOs.Frontend;

public class PlayerDto
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public bool IsHost { get; set; }

    [JsonConstructor]
    public PlayerDto(int userId, string username, bool isHost)
    {
        UserId = userId;
        Username = username;
        IsHost = isHost;
    }
    
    public PlayerDto(User user, bool isHost = false)
    {
        UserId = user.Id;
        Username = user.UserName;
        IsHost = isHost;
    }
}
