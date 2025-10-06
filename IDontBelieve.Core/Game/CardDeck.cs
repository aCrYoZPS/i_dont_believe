using IDontBelieve.Core.Models;

namespace IDontBelieve.Core.Game;

public class CardDeck
{
    public static List<Card> CreateDeck(DeckType deckType)
    {
        var deck = new List<Card>();
        foreach (CardSuit suit in Enum.GetValues<CardSuit>())
        {
            var startRank = deckType == DeckType.Cards36 ? CardRank.Six : CardRank.Two;
            var endRank = CardRank.Ace;

            for (var rank = startRank; rank <= endRank; rank++)
            {
                if (rank == CardRank.Joker) continue;
                deck.Add(new Card {Suit = suit, Rank = rank});
            }
        }

        if (deckType == DeckType.Cards54)
        {
            deck.Add(new Card { Suit = CardSuit.Hearts, Rank = CardRank.Joker });
            deck.Add(new Card { Suit = CardSuit.Spades, Rank = CardRank.Joker });
        }

        return deck;
    }

    public static List<Card> Shuffle(List<Card> deck)
    {
        var random = new Random();
        var shuffled = new List<Card>(deck);

        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }
        return shuffled;
    }

    public static List<List<Card>> DealCards(List<Card> deck, int playerCount)
    {
        var hands = new List<List<Card>>();
        for (int i = 0; i < playerCount; i++)
        {
            hands.Add(new List<Card>());
        }

        var cardsPerPlayer = deck.Count / playerCount;
        var cardIndex = 0;

        for (int round = 0; round < cardsPerPlayer; round++)
        {
            for (int player = 0; player < playerCount; player++)
            {
                if (cardIndex < deck.Count)
                {
                    hands[player].Add(deck[cardIndex++]);
                }
            } 
        }

        return hands;
    }
}