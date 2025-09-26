using IDontBelieve.Core.Models;
using IDontBelieve.Core.DTOs;

namespace IDontBelieve.Core.Services;

public interface IGameRoomService
{
    Task<GameRoom> CreateRoomAsync(CreateRoomDto dto, int userId);
    Task<List<GameRoom>> GetAvailableRoomsAsync(RoomFilterDto filter);
    Task<GameRoom?> GetRoomByIdAsync(int roomId);
    Task<GameRoom?> GetRoomWithPlayersAsync(int roomId);
    Task<JoinRoomResultDto> JoinRoomAsync(int roomId, int userId);
    Task<bool> LeaveRoomAsync(int roomId, int userId);
    Task<bool> CanStartGameAsync(int roomId);
    Task UpdateRoomStatusAsync(int roomId, GameRoomStatus status);
    Task<List<User>> GetRoomPlayersAsync(int roomId);
}