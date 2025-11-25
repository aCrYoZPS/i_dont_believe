namespace IDontBelieve.Core.Models;

public class LeaderboardUpdate
{
    public List<LeaderboardUser> Users { get; set; } = new();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class LeaderboardUser
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public int GamesPlayed { get; set; }
    public int GamesWon { get; set; }
    public double WinRate { get; set; }
    public int Rank { get; set; }
}