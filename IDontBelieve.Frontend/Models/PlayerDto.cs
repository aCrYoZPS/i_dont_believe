namespace IDontBelieve.Frontend.Models;

public class PlayerDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsHost { get; set; }
}
