using IDontBelieve.Core.Models;
using IDontBelieve.Core.DTOs;

namespace IDontBelieve.Core.Services;

public interface IGameRoomService
{
    GameRoom CreateRoomAsync(CreateRoomDto dto, int userId);
    List<GameRoom> GetAvailableRoomsAsync(RoomFilterDto filter);
    GameRoom? GetRoomByIdAsync(int roomId);
    GameRoom? GetRoomWithPlayersAsync(int roomId);
    JoinRoomResultDto JoinRoomAsync(int roomId, int userId);
    bool LeaveRoomAsync(int roomId, int userId);
    bool CanStartGameAsync(int roomId);
    void UpdateRoomStatusAsync(int roomId, GameRoomStatus status);
    List<User> GetRoomPlayersAsync(int roomId);
}