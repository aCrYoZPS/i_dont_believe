namespace IDontBelieve.Core.DTOs.Frontend;
using System.Text.Json.Serialization;
using IDontBelieve.Core.Models;

public class CardDto
{
    public CardSuit Suit { get; set; }
    public CardRank Rank { get; set; }
    public bool IsFaceUp { get; set; } = true;
    public bool IsSelected { get; set; }
    
    public CardDto() { }
    
    [JsonConstructor]
    public CardDto(CardSuit suit, CardRank rank, bool isFaceUp = true, bool isSelected = false)
    {
        Suit = suit;
        Rank = rank;
        IsFaceUp = isFaceUp;
        IsSelected = isSelected;
    }
}