using Microsoft.Extensions.Logging;
using IDontBelieve.Core.Models;
using IDontBelieve.Core.Services;
using IDontBelieve.Core.DTOs;
using IDontBelieve.Infrastructure.Repositories;

namespace IDontBelieve.Infrastructure.Services;

public class GameRoomService : IGameRoomService
{
    private readonly ILogger<GameRoomService> _logger;
    private readonly List<GameRoom> _rooms = new();
    private int _nextRoomId;

    public GameRoomService(ILogger<GameRoomService> logger)
    {
        _logger = logger;
    }

    public GameRoom CreateRoomAsync(CreateRoomDto dto, User user)
    {
        var room = new GameRoom
        {
            Id = Interlocked.Increment(ref _nextRoomId),
            Name = dto.Name,
            MaxPlayers = dto.MaxPlayers,
            DeckType = dto.DeckType,
            ShowCardCount = dto.ShowCardCount,
            Status = GameRoomStatus.Waiting,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = user.Id,
            CreatedBy = user
        };

        _rooms.Add(room);
        JoinRoomInternal(room, user);

        _logger.LogInformation("Created room {RoomName} (ID: {RoomId}) by user {UserId}",
            dto.Name, room.Id, user.Id);

        return room;
    }

    public List<GameRoom> GetRooms() => _rooms;

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

        return query.OrderBy(r => r.CreatedAt).ToList();
    }

    public GameRoom? GetRoomByIdAsync(int roomId)
    {
        return _rooms.FirstOrDefault(r => r.Id == roomId);
    }

    public GameRoom? GetRoomWithPlayersAsync(int roomId) =>
        GetRoomByIdAsync(roomId);

    public JoinRoomResultDto JoinRoomAsync(int roomId, User user)
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

        return JoinRoomInternal(room, user);
    }

    private JoinRoomResultDto JoinRoomInternal(GameRoom room, User user)
    {
        if (room.Status != GameRoomStatus.Waiting)
        {
            return new JoinRoomResultDto { Success = false, Message = "Game has already started" };
        }

        if (room.Players.Count >= room.MaxPlayers)
        {
            return new JoinRoomResultDto { Success = false, Message = "Room is full" };
        }

        if (room.Players.Any(p => p.UserId == user.Id))
        {
            return new JoinRoomResultDto { Success = true, Message = "You are already in the room", Room = room };
        }

        var player = new GamePlayer
        {
            GameRoomId = room.Id,
            GameRoom = room,
            UserId = user.Id,
            Position = room.Players.Count,
            Status = PlayerStatus.Waiting,
            User = user
        };

        room.Players.Add(player);

        _logger.LogInformation("User {UserId} joined room {RoomId}", user.Id, room.Id);

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
            _logger.LogInformation("Room {RoomId} deleted", roomId);
        }

        _logger.LogInformation("User {UserId} left room {RoomId}", userId, roomId);
        return true;
    }


    public bool CanStartGameAsync(int roomId)
    {
        var room = _rooms.FirstOrDefault(r => r.Id == roomId);
        return room != null && room.CanStart && room.Status == GameRoomStatus.Waiting;
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

        return room.Players
            .Select(p => p.User)
            .Where(u => u != null)
            .ToList();
    }
}