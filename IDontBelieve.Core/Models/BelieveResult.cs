namespace IDontBelieve.Core.Models;

public class BelieveResult
{
    public bool CardsMatchClaim { get; set; }
    public List<Card> CardsRevealed { get; set; } = new();
    public CardRank ClaimedRank { get; set; }
    public string Description => CardsMatchClaim 
        ? "Player was telling the truth" 
        : "Player was lying";
}