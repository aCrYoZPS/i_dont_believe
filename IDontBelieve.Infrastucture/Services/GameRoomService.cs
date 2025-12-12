using Microsoft.Extensions.Logging;
using IDontBelieve.Core.Models;
using IDontBelieve.Core.Services;
using IDontBelieve.Core.DTOs;

namespace IDontBelieve.Infrastructure.Services;

public class GameRoomService 
: IGameRoomService
{
    private readonly ILogger<GameRoomService> _logger;
    private readonly IUserService _userService;
    private readonly List<GameRoom> _rooms = new();

    public GameRoomService(ILogger<GameRoomService> logger,  IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public GameRoom CreateRoomAsync(CreateRoomDto dto, int userId)
    {
        var room = new GameRoom
        {
            Name = dto.Name,
            MaxPlayers = dto.MaxPlayers,
            DeckType = dto.DeckType,
            ShowCardCount = dto.ShowCardCount,
            Status = GameRoomStatus.Waiting,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId
        };
        
        _rooms.Add(room);
        
        JoinRoomAsync(room.Id, userId);

        _logger.LogInformation("Created room {RoomName} (ID: {RoomId}) by user {UserId}", 
            dto.Name, room.Id, userId);
        
        return room;
    }

    public List<GameRoom> GetAvailableRoomsAsync(RoomFilterDto filter)
    {
        var query = _rooms.AsQueryable();
        
        if (filter.ShowOnlyJoinable == true)
        {
            query = query.Where(r => r.Status == GameRoomStatus.Waiting);
        }
        
        if (filter.DeckType.HasValue)
        {
            query = query.Where(r => r.DeckType == filter.DeckType.Value);
        }
        
        if (filter.MaxPlayers.HasValue)
        {
            query = query.Where(r => r.MaxPlayers == filter.MaxPlayers.Value);
        }
        
        if (!string.IsNullOrEmpty(filter.RoomName))
        {
            query = query.Where(r => r.Name.Contains(filter.RoomName, StringComparison.OrdinalIgnoreCase));
        }
        
        if (filter.ShowOnlyJoinable == true)
        {
            query = query.Where(r => r.Players.Count < r.MaxPlayers);
        }
        
        var result = query.OrderBy(r => r.CreatedAt).ToList();

        return result;
    }

    public GameRoom? GetRoomByIdAsync(int roomId)
    
    {
        return _rooms.FirstOrDefault(r => r.Id == roomId);
    }

    public GameRoom? GetRoomWithPlayersAsync(int roomId) => 
        GetRoomByIdAsync(roomId);
    
    public JoinRoomResultDto JoinRoomAsync(int roomId, int userId)
    {
        var room = _rooms.FirstOrDefault(r => r.Id == roomId);

        if (room == null)
        {
            return new JoinRoomResultDto
            {
                Success = false,
                Message = "Room not found"
            };
        }
        
        var result = JoinRoomInternal(room, userId);
        return result;
    }
    
    private JoinRoomResultDto JoinRoomInternal(GameRoom room, int userId)
    {
        if (room.Status != GameRoomStatus.Waiting)
        {
            return new JoinRoomResultDto { Success = false, Message = "Game has already started" };
        }

        if (room.Players.Count >= room.MaxPlayers)
        {
            return new JoinRoomResultDto { Success = false, Message = "Room is full" };
        }

        if (room.Players.Any(p => p.UserId == userId))
        {
            return new JoinRoomResultDto { Success = true, Message = "You are already in the room", Room = room };
        }

        var player = new GamePlayer
        {
            GameRoomId = room.Id,
            GameRoom = room,
            UserId = userId,
            Position = room.Players.Count,
            Status = PlayerStatus.Waiting,
            User = _userService.GetUserByIdAsync(userId).Result,
        };

        room.Players.Add(player);

        _logger.LogInformation("User {UserId} joined room {RoomId}", userId, room.Id);
        
        return new JoinRoomResultDto
        {
            Success = true,
            Message = "Connection to room is successful",
            Room = room
        };
    }

    public bool LeaveRoomAsync(int roomId, int userId)
    {
        var room = _rooms.FirstOrDefault(r => r.Id == roomId);
        if (room == null) return false;

        var player = room.Players.FirstOrDefault(p => p.UserId == userId);
        if (player == null) return false;
        
        room.Players.Remove(player);
        
        if (room.Players.Count == 0) 
        {
            _rooms.Remove(room);
        }

        _logger.LogInformation("User {UserId} left room {RoomId}", userId, roomId);
        return true;
    }


    public bool CanStartGameAsync(int roomId)
    {
        var room = _rooms.FirstOrDefault(r => r.Id == roomId);
        bool canStart = room != null && room.CanStart && room.Status == GameRoomStatus.Waiting;
        return canStart;
    }

    public void UpdateRoomStatusAsync(int roomId, GameRoomStatus status)
    {
        var room = _rooms.FirstOrDefault(r => r.Id == roomId);
        if (room != null)
        {
            room.Status = status;
            _logger.LogInformation("Room {RoomId} status updated to {Status}", roomId, status);
        }
    }

    public List<User> GetRoomPlayersAsync(int roomId)
    {
        
        var room = _rooms.FirstOrDefault(r => r.Id == roomId);
        if (room == null)
        {
            return new List<User>();
        }

        var users = room.Players
            .Select(p => p.User)
            .Where(u => u != null)
            .ToList();

        return users;
    }
}