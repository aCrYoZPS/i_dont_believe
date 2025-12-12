namespace IDontBelieve.Core.DTOs.Frontend;

public class CreateRoomDto
{
    public string Name { get; set; } = string.Empty;
    public int MaxPlayers { get; set; }
}