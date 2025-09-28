namespace IDontBelieve.Core.Services;

public interface INotificationService
{
    Task NotifyRoomPlayersAsync(int roomId, string method, object data);
    Task NotifyPlayerAsync(int userId, string method, object data);
    Task NotifyAllPlayersAsync(string method, object data);
}