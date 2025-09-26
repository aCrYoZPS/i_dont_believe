using IDontBelieve.Core.Models;

namespace IDontBelieve.Core.Game;

public static class GameLogic
{
    public static bool IsValidMove(List<Card> playerHand, List<Card> cardsToPlay, CardRank claimedRank)
    {
        foreach (var card in cardsToPlay)
        {
            if (!playerHand.Contains(card))
                return false;
        }
        
        if (cardsToPlay.Count == 0)
            return false;
            
        return true;
    }
    
    public static BelieveResult ProcessBelieveChallenge(List<Card> cardsPlayed, CardRank claimedRank)
    {
        bool allCardsMatch = cardsPlayed.All(card => card.Rank == claimedRank || card.IsJoker);
        
        return new BelieveResult
        {
            CardsMatchClaim = allCardsMatch,
            CardsRevealed = cardsPlayed,
            ClaimedRank = claimedRank
        };
    }
    
    public static bool IsGameEnded(List<GamePlayer> players)
    {
        return players.Any(p => p.Hand.Count == 0 && p.Status == PlayerStatus.Playing);
    }
    
    public static int GetWinnerId(List<GamePlayer> players)
    {
        var winner = players.FirstOrDefault(p => p.Hand.Count == 0 && p.Status == PlayerStatus.Playing);
        return winner?.UserId ?? -1;
    }
    
    public static int GetNextPlayerId(List<GamePlayer> players, int currentPlayerId)
    {
        var activePlayers = players
            .Where(p => p.Status == PlayerStatus.Playing)
            .OrderBy(p => p.Position)
            .ToList();
            
        if (activePlayers.Count <= 1)
            return currentPlayerId;
            
        var currentIndex = activePlayers.FindIndex(p => p.UserId == currentPlayerId);
        if (currentIndex == -1)
            return activePlayers[0].UserId;
            
        var nextIndex = (currentIndex + 1) % activePlayers.Count;
        return activePlayers[nextIndex].UserId;
    }
    
    public static GamePlayer? GetPlayerById(List<GamePlayer> players, int playerId)
    {
        return players.FirstOrDefault(p => p.UserId == playerId);
    }
    
    public static List<Card> CombineDiscardPile(List<Card> currentDiscard, List<Card> newCards)
    {
        var combined = new List<Card>(currentDiscard);
        combined.AddRange(newCards);
        return combined;
    }
}