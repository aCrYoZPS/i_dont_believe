namespace IDontBelieve.Core.Models;

public enum DeckType
{
    Cards36 = 36,
    Cards52 = 52,
    Cards54 = 54,    
}

public enum GameRoomStatus
{
    Waiting = 0,
    InProgress = 1,
    Finished = 2,
    Cancelled = 3,
}

public enum PlayerStatus
{
    Waiting = 0,
    Playing = 1,
    Eliminated = 2,
    Winner = 3,
    Disconnected = 4,
}

public enum GamePhase
{
    WaitingForPlayers = 0,
    Dealing = 1,
    Playing = 2,
    Finished = 3,
}

public enum MoveOutcome
{
    Continue = 0,
    Believe = 1,
    NotBelieve = 2,
    RoundEnd = 3,
    GameEnd = 4,
}

public enum CardSuit
{
    Hearts = 0,
    Diamonds = 1,
    Clubs = 2,
    Spades = 3,
}

public enum CardRank
{
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8, 
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13,
    Ace = 14,
    Joker = 15
}