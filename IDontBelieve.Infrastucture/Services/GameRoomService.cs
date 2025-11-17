using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IDontBelieve.Core.Models;
using IDontBelieve.Core.Services;
using IDontBelieve.Core.DTOs;
using IDontBelieve.Infrastructure.Data;

namespace IDontBelieve.Infrastructure.Services;

public class GameRoomService {}
/*
: IGameRoomService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GameRoomService> _logger;

    public GameRoomService(ApplicationDbContext context, ILogger<GameRoomService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GameRoom> CreateRoomAsync(CreateRoomDto dto, int userId)
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

        _context.GameRooms.Add(room);
        await _context.SaveChangesAsync();

        await JoinRoomAsync(room.Id, userId);

        _logger.LogInformation("Created room {RoomName} (ID: {RoomId}) by user {UserId}", 
            dto.Name, room.Id, userId);
        
        return room;
    }

    public async Task<List<GameRoom>> GetAvailableRoomsAsync(RoomFilterDto filter)
    {
        var query = _context.GameRooms
            .Include(r => r.Players)
            .ThenInclude(p => p.User)
            .Include(r => r.CreatedBy)
            .AsQueryable();

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
            query = query.Where(r => r.Name.Contains(filter.RoomName));
        }

        if (filter.ShowOnlyJoinable == true)
        {
            query = query.Where(r => r.Players.Count < r.MaxPlayers);
        }

        return await query
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<GameRoom?> GetRoomByIdAsync(int roomId)
    {
        return await _context.GameRooms
            .FirstOrDefaultAsync(r => r.Id == roomId);
    }

    public async Task<GameRoom?> GetRoomWithPlayersAsync(int roomId)
    {
        return await _context.GameRooms
            .Include(r => r.Players)
            .ThenInclude(p => p.User)
            .Include(r => r.CreatedBy)
            .FirstOrDefaultAsync(r => r.Id == roomId);
    }

    public async Task<JoinRoomResultDto> JoinRoomAsync(int roomId, int userId)
    {
        var room = await GetRoomWithPlayersAsync(roomId);
        if (room == null)
        {
            return new JoinRoomResultDto
            {
                Success = false,
                Message = "Room not found"
            };
        }

        if (room.Status != GameRoomStatus.Waiting)
        {
            return new JoinRoomResultDto
            {
                Success = false,
                Message = "Game has already started"
            };
        }

        if (room.Players.Count >= room.MaxPlayers)
        {
            return new JoinRoomResultDto
            {
                Success = false,
                Message = "Room is full"
            };
        }

        if (room.Players.Any(p => p.UserId == userId))
        {
            return new JoinRoomResultDto
            {
                Success = true,
                Message = "You are already in the room",
                Room = room
            };
        }

        var player = new GamePlayer
        {
            GameRoomId = roomId,
            UserId = userId,
            Position = room.Players.Count,
            Status = PlayerStatus.Waiting
        };

        _context.GamePlayers.Add(player);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} joined room {RoomId}", userId, roomId);

        room = await GetRoomWithPlayersAsync(roomId);
        
        return new JoinRoomResultDto
        {
            Success = true,
            Message = "Connection to room is successful",
            Room = room
        };
    }

    public async Task<bool> LeaveRoomAsync(int roomId, int userId)
    {
        var player = await _context.GamePlayers
            .FirstOrDefaultAsync(p => p.GameRoomId == roomId && p.UserId == userId);

        if (player == null) return false;

        _context.GamePlayers.Remove(player);

        var playersLeft = await _context.GamePlayers
            .CountAsync(p => p.GameRoomId == roomId);

        if (playersLeft == 1) 
        {
            var room = await _context.GameRooms.FindAsync(roomId);
            if (room != null)
            {
                _context.GameRooms.Remove(room);
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} left room {RoomId}", userId, roomId);
        return true;
    }

    public async Task<bool> CanStartGameAsync(int roomId)
    {
        var room = await GetRoomWithPlayersAsync(roomId);
        return room != null && room.CanStart && room.Status == GameRoomStatus.Waiting;
    }

    public async Task UpdateRoomStatusAsync(int roomId, GameRoomStatus status)
    {
        var room = await GetRoomByIdAsync(roomId);
        if (room != null)
        {
            room.Status = status;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Room {RoomId} status updated to {Status}", roomId, status);
        }
    }

    public async Task<List<User>> GetRoomPlayersAsync(int roomId)
    {
        return await _context.GamePlayers
            .Where(p => p.GameRoomId == roomId)
            .Include(p => p.User)
            .Select(p => p.User)
            .ToListAsync();
    }
}*/