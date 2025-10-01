using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using IDontBelieve.Core.Services;


namespace IDontBelieve.Infrastructure.Services;

public class NotificationService : INotificationService
{
    public NotificationService() { }

    public async Task NotifyRoomPlayersAsync(int roomId, string method, object data) { }

    public async Task NotifyPlayerAsync(int userId, string method, object data) { }


    public async Task NotifyAllPlayersAsync(string method, object data) { }
}