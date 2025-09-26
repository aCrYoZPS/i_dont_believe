namespace IDontBelieve.Core.Game;

public static class GameRules
{
    public const int MinPlayers = 3;
    public const int MaxPlayers = 6;
    public const int DefaultCoins = 1000;
    public const int DefaultRating = 1000;
    public const int MaxRoomNameLength = 100;
    public const int MinRoomNameLength = 3;
    
    public const decimal GameEntryFee = 10m;
    public const decimal WinnerReward = 50m;
    public const int RatingWinBonus = 25;
    public const int RatingLossBonus = -10;
    
    public static readonly TimeSpan MoveTimeout = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan GameTimeout = TimeSpan.FromHours(1);
    public static readonly TimeSpan RoomWaitTimeout = TimeSpan.FromMinutes(10);
}