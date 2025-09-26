using System.Security.AccessControl;
using System.Text.Json.Serialization;

namespace IDontBelieve.Core.Models;

public class Card
{
    public CardSuit Suit { get; set; }
    public CardRank Rank { get; set; }

    [JsonIgnore] public bool IsJoker => Rank == CardRank.Joker;

    [JsonIgnore] public string DisplayName => IsJoker ? "Joker" : $"{GetRankName()} {GetSuitName()}";
        
    [JsonIgnore] public string ShortName => IsJoker ? "JK" : $"{GetRankShort()}{GetSuitShort()}";

    public override string ToString() => DisplayName;

    public override bool Equals(object? obj) =>
        obj is Card card && Suit == card.Suit && Rank == card.Rank;

    public override int GetHashCode() => HashCode.Combine(Suit, Rank);
    
    private string GetRankName() => Rank switch
    {
        
        CardRank.Two => "Two",
        CardRank.Three => "Three",
        CardRank.Four => "Four",
        CardRank.Five => "Five", 
        CardRank.Six => "Six",
        CardRank.Seven => "Seven", 
        CardRank.Eight => "Eight",
        CardRank.Nine => "Nine",
        CardRank.Ten => "Ten",
        CardRank.Jack => "Jack",
        CardRank.Queen => "Queen", 
        CardRank.King => "King",
        CardRank.Ace => "Ace",
        _ => "?"
    };
    
    private string GetSuitName() => Suit switch
    {
        CardSuit.Hearts => "Hearts",
        CardSuit.Diamonds => "Diamonds",
        CardSuit.Clubs => "Clubs", 
        CardSuit.Spades => "Spades",
        _ => ""
    };
    
    private string GetRankShort() => Rank switch
    {
        CardRank.Two => "2",
        CardRank.Three => "3", 
        CardRank.Four => "4",
        CardRank.Five => "5",
        CardRank.Six => "6",
        CardRank.Seven => "7", 
        CardRank.Eight => "8",
        CardRank.Nine => "9",
        CardRank.Ten => "10",
        CardRank.Jack => "В",
        CardRank.Queen => "Д", 
        CardRank.King => "К",
        CardRank.Ace => "Т",
        CardRank.Joker => "JK",
        _ => "?"
    };
    
    private string GetSuitShort() => Suit switch
    {
        CardSuit.Hearts => "♥",
        CardSuit.Diamonds => "♦",
        CardSuit.Clubs => "♣", 
        CardSuit.Spades => "♠",
        _ => ""
    };
}