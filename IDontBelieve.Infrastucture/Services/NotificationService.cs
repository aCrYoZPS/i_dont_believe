using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using IDontBelieve.Core.Services;
using IDontBelieve.Infrastructure.Hubs;

namespace IDontBelieve.Infrastructure.Services;
public class NotificationService : INotificationService
{
    private readonly IHubContext<GameHub> _gameHub;
    private readonly IHubContext<GameRoomHub> _roomHub;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IHubContext<GameHub> gameHub,
        IHubContext<GameRoomHub> roomHub,
        ILogger<NotificationService> logger)
    {
        _gameHub = gameHub;
        _roomHub = roomHub;
        _logger = logger;
    }

    public async Task NotifyRoomPlayersAsync(int roomId, string method, object data)
    {
        try
        {
            var groupName = $"Game_{roomId}";
            await _gameHub.Clients.Group(groupName).SendAsync(method, data);
            
            _logger.LogDebug("Notified room {RoomId} with method {Method}", roomId, method);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify room {RoomId} with method {Method}", roomId, method);
        }
    }
    
    public async Task NotifyPlayerAsync(int userId, string method, object data)
    {
        try
        {
            var connectionId = GameHub.GetUserConnectionId(userId);
            
            if (!string.IsNullOrEmpty(connectionId))
            {
                await _gameHub.Clients.Client(connectionId).SendAsync(method, data);
                _logger.LogDebug("Notified user {UserId} with method {Method}", userId, method);
            }
            else
            {
                _logger.LogWarning("Connection not found for user {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify user {UserId} with method {Method}", userId, method);
        }
    }

    public async Task NotifyAllPlayersAsync(string method, object data)
    {
        try
        {
            await _gameHub.Clients.All.SendAsync(method, data);
            _logger.LogDebug("Broadcast notification with method {Method}", method);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast with method {Method}", method);
        }
    }

    public async Task NotifyLobbyAsync(string method, object data)
    {
        try
        {
            await _roomHub.Clients.Group("Lobby").SendAsync(method, data);
            _logger.LogDebug("Notified lobby with method {Method}", method);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify lobby with method {Method}", method);
        }
    }

    public async Task NotifyRoomLobbyAsync(int roomId, string method, object data)
    {
        try
        {
            var groupName = $"Room_{roomId}";
            await _roomHub.Clients.Group(groupName).SendAsync(method, data);
            _logger.LogDebug("Notified room lobby {RoomId} with method {Method}", roomId, method);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify room lobby {RoomId} with method {Method}", roomId, method);
        }
    }

    public async Task NotifyPlayerErrorAsync(int userId, string errorMessage)
    {
        await NotifyPlayerAsync(userId, "Error", new { Message = errorMessage, Timestamp = DateTime.UtcNow });
    }
    
    public async Task NotifyPlayerSuccessAsync(int userId, string successMessage)
    {
        await NotifyPlayerAsync(userId, "Success", new { Message = successMessage, Timestamp = DateTime.UtcNow });
    }
}
