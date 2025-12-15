using Microsoft.AspNetCore.SignalR;
using IDontBelieve.Core.Services;
using IDontBelieve.Core.DTOs;
using Microsoft.Extensions.Logging; 
using IDontBelieve.Core.DTOs.Frontend;
using IDontBelieve.Infrastructure.Repositories;

namespace IDontBelieve.Infrastructure.Hubs;

public class GameRoomHub : Hub
{
    private readonly IGameRoomService _gameRoomService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GameRoomHub> _logger;

    private static readonly Dictionary<string, int> _connectionToUser = new();
    private static readonly Dictionary<int, string> _userToConnection = new();
    private static readonly Dictionary<int, HashSet<string>> _roomConnections = new();

    public GameRoomHub(IGameRoomService gameRoomService, IUserRepository userRepository, ILogger<GameRoomHub> logger)
    {
        _gameRoomService = gameRoomService;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task JoinLobby(int userId, string username)
    {
        try
        {
            _connectionToUser[Context.ConnectionId] = userId;
            _userToConnection[userId] = Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, "Lobby");
            _logger.LogError("User {Username} ({UserId}) joined lobby", username, userId);
            
            await Clients.OthersInGroup("Lobby").SendAsync("PlayerJoinedLobby", new { 
                UserId = userId, 
                Username = username,
                Timestamp = DateTime.UtcNow
            });

            var rooms = _gameRoomService.GetAvailableRoomsAsync(new RoomFilterDto());
            var listDto = new List<GameRoomDto>();
            foreach (var room in rooms)
            {
                listDto.Add(new GameRoomDto(room));
            }
            await Clients.Caller.SendAsync("RoomsList", listDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in JoinLobby for user {UserId}", userId);
            await Clients.Caller.SendAsync("Error", "Failed to join lobby");
        }
    }

    public async Task LeaveLobby(int userId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Lobby");
            
            _connectionToUser.Remove(Context.ConnectionId);
            _userToConnection.Remove(userId);
            
            await Clients.OthersInGroup("Lobby").SendAsync("PlayerLeftLobby", new { 
                UserId = userId,
                Timestamp = DateTime.UtcNow
            });
            
            _logger.LogError("User {UserId} left lobby", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in LeaveLobby for user {UserId}", userId);
        }
    }

    public async Task CreateRoom(IDontBelieve.Core.DTOs.CreateRoomDto roomDto, int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            var room = _gameRoomService.CreateRoomAsync(roomDto, user);

            var dto = new IDontBelieve.Core.DTOs.Frontend.GameRoomDto(room);
            
            await Clients.Caller.SendAsync("RoomCreated", dto);
            
            var rooms = _gameRoomService.GetAvailableRoomsAsync(new RoomFilterDto());
            var listDto = new List<IDontBelieve.Core.DTOs.Frontend.GameRoomDto>();
            foreach (var r in rooms)
            {
                listDto.Add(new GameRoomDto(r));
            }
            await Clients.OthersInGroup("Lobby").SendAsync("RoomsList", listDto);
            
            _logger.LogError("Room {RoomName} (ID: {RoomId}) created by user {UserId}, all rooms: {sas}, free: {sas2}", 
                room.Name, room.Id, userId, _gameRoomService.GetRooms().Count, listDto.Count);
            _logger.LogWarning($"Object IDs: Hub : {this.GetHashCode()}, RoomServ: {_gameRoomService.GetHashCode()}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create room for user {UserId}", userId);
            await Clients.Caller.SendAsync("RoomCreationFailed", new {
                Message = "Failed to create room",
                Error = ex.Message
            });
        }
    }

    public async Task JoinRoom(int roomId, int userId, string username)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            var result = _gameRoomService.JoinRoomAsync(roomId, user);
            
            if (!result.Success)
            {
                await Clients.Caller.SendAsync("JoinRoomFailed", new {
                    Message = result.Message,
                    RoomId = roomId
                });
                return;
            }

            var groupName = $"Room_{roomId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            if (!_roomConnections.ContainsKey(roomId))
            {
                _roomConnections[roomId] = new HashSet<string>();
            }
            _roomConnections[roomId].Add(Context.ConnectionId);
            
            _logger.LogInformation("User {Username} joined room {RoomId}", username, roomId);
            
            await Clients.Group(groupName).SendAsync("PlayerJoinedRoom", new { 
                RoomId = roomId,
                UserId = userId, 
                Username = username,
                RoomDto = new GameRoomDto(result.Room),
                Timestamp = DateTime.UtcNow
            });
            
            _logger.LogError("1");
            
            await Clients.Group("Lobby").SendAsync("RoomUpdated", new GameRoomDto(result.Room));
            
            _logger.LogError("2");
            
            await Clients.Caller.SendAsync("JoinRoomSuccess", new {
                RoomDto = new GameRoomDto(result.Room),
                Message = "Joined room successfully"
            });
            
            _logger.LogError("3");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to join room {RoomId} for user {UserId}", roomId, userId);
            await Clients.Caller.SendAsync("JoinRoomFailed", new {
                Message = "Failed to join room",
                Error = ex.Message,
                RoomId = roomId
            });
        }
    }

    public async Task LeaveRoom(int roomId, int userId, string username)
    {
        try
        {
            var groupName = $"Room_{roomId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            
            if (_roomConnections.ContainsKey(roomId))
            {
                _roomConnections[roomId].Remove(Context.ConnectionId);
            }
            
            _gameRoomService.LeaveRoomAsync(roomId, userId);
            
            _logger.LogInformation("User {Username} left room {RoomId}", username, roomId);
            
            await Clients.Group(groupName).SendAsync("PlayerLeftRoom", new { 
                RoomId = roomId,
                UserId = userId, 
                Username = username,
                Timestamp = DateTime.UtcNow
            });
            
            var room = _gameRoomService.GetRoomByIdAsync(roomId);
            if (room != null)
            {
                await Clients.Group("Lobby").SendAsync("RoomUpdated", room);
            }
            else
            {
                await Clients.Group("Lobby").SendAsync("RoomDeleted", new { RoomId = roomId });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to leave room {RoomId} for user {UserId}", roomId, userId);
        }
    }

    public async Task StartGame(int roomId, int userId)
    {
        try
        {
            var canStart = _gameRoomService.CanStartGameAsync(roomId);
            if (!canStart)
            {
                await Clients.Caller.SendAsync("GameStartFailed", new {
                    Message = "Cannot start game - not enough players or game already started",
                    RoomId = roomId
                });
                return;
            }

            var groupName = $"Room_{roomId}";
            _logger.LogInformation("Game starting in room {RoomId} by user {UserId}", roomId, userId);
            
            await Clients.Group(groupName).SendAsync("GameStarting", new { 
                RoomId = roomId,
                InitiatorId = userId,
                Timestamp = DateTime.UtcNow
            });
            
            _gameRoomService.UpdateRoomStatusAsync(roomId, 
                IDontBelieve.Core.Models.GameRoomStatus.InProgress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Failed to start game in room {RoomId}", roomId);
            await Clients.Caller.SendAsync("GameStartFailed", new {
                Message = "Failed to start game",
                Error = ex.Message,
                RoomId = roomId
            });
        }
    }

    public async Task SendRoomMessage(int roomId, int userId, string username, string message)
    {
        try
        {
            var groupName = $"Room_{roomId}";
            await Clients.Group(groupName).SendAsync("RoomMessage", new
            {
                UserId = userId,
                Username = username,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message in room {RoomId}", roomId);
        }
    }

    public async Task GetAvailableRooms(RoomFilterDto? filter)
    {
        try
        {
            filter ??= new RoomFilterDto();
            var rooms = _gameRoomService.GetAvailableRoomsAsync(filter);
            await Clients.Caller.SendAsync("RoomsList", rooms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available rooms");
            await Clients.Caller.SendAsync("Error", "Failed to load rooms");
        }
    }

    public async Task GetRoomDetails(int roomId)
    {
        try
        {
            var room = _gameRoomService.GetRoomWithPlayersAsync(roomId);
            if (room != null)
            {
                await Clients.Caller.SendAsync("RoomDetails", new GameRoomDto(room));
            }
            else
            {
                await Clients.Caller.SendAsync("RoomNotFound", new { RoomId = roomId });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get room details for room {RoomId}", roomId);
            await Clients.Caller.SendAsync("Error", "Failed to load room details");
        }
    }

    public Task<GameRoomDto?> GetRoomDetailsSync(int roomId)
    {
        try
        {
            var room = _gameRoomService.GetRoomWithPlayersAsync(roomId);
            return Task.FromResult(room != null ? new GameRoomDto(room) : null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get room details sync for room {RoomId}", roomId);
            return Task.FromResult<GameRoomDto?>(null);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connectionToUser.TryGetValue(Context.ConnectionId, out var userId))
        {
            _connectionToUser.Remove(Context.ConnectionId);
            _userToConnection.Remove(userId);
            
            foreach (var roomConnections in _roomConnections.Values)
            {
                roomConnections.Remove(Context.ConnectionId);
            }
            
            await Clients.Group("Lobby").SendAsync("PlayerDisconnected", new { UserId = userId });
            
            _logger.LogInformation("User {UserId} disconnected", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }
    

    public static string? GetUserConnectionId(int userId)
    {
        return _userToConnection.GetValueOrDefault(userId);
    }
    
    public static int? GetUserIdFromConnection(string connectionId)
    {
        return _connectionToUser.GetValueOrDefault(connectionId);
    }

}
