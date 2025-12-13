using System.ComponentModel.DataAnnotations;
using IDontBelieve.Core.Models;

namespace IDontBelieve.Core.DTOs;

public class CreateRoomDto
{
    [Required, StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Range(3, 6)] public int MaxPlayers { get; set; } = 4;
    public DeckType DeckType { get; set; } = DeckType.Cards36;

    public int HostId { get; set; } = -1;
    public bool ShowCardCount { get; set; } = true;
}