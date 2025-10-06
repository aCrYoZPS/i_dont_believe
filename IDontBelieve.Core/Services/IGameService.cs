using IDontBelieve.Core.Models;
using IDontBelieve.Core.DTOs;

namespace IDontBelieve.Core.Services;

public interface IGameService
{
    Task<bool> StartGameAsync(int roomId);
    Task<GameMoveResultDto> MakeMoveAsync(int roomId, int playerId, GameMoveDto move);
    Task<GameStateDto?> GetGameStateAsync(int roomId);
    Task<GameMoveResultDto> BelieveOrNotAsync(int roomId, int playerId, bool believe);
    Task EndGameAsync(int roomId, int winnerId);
    Task<bool> IsValidMoveAsync(int roomId, int playerId, GameMoveDto move);
    Task<bool> IsPlayerTurnAsync(int roomId, int playerId);
}