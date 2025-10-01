using Microsoft.AspNetCore.SignalR;
using IDontBelieve.Core.Services;
using IDontBelieve.Core.Models;
using IDontBelieve.Core.DTOs;
using Microsoft.Extensions.Logging; 

namespace IDontBelieve.Infrastructure.Hubs;

public class GameHub : Hub
{
    private readonly IGameService _gameService;
    private readonly IUserService _userService;
    private readonly ILogger<GameHub> _logger;

    private static readonly Dictionary<string, int> _connectionToUser = new();
    private static readonly Dictionary<int, string> _userToConnection = new();

    public GameHub(IGameService gameService, IUserService userService, ILogger<GameHub> logger)
    {
        _gameService = gameService;
        _userService = userService;
        _logger = logger;
    }

    public async Task JoinGame(int roomId, int userId, string username)
    {
        try
        {
            _connectionToUser[Context.ConnectionId] = userId;
            _userToConnection[userId] = Context.ConnectionId;

            var groupName = $"Game_{roomId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation("User {Username} joined game in room {RoomId}", username, roomId);

            var gameState = await _gameService.GetGameStateAsync(roomId);
            if (gameState != null)
            {
                await Clients.Caller.SendAsync("GameStateUpdate", gameState);
            }

            await Clients.OthersInGroup(groupName).SendAsync("PlayerJoinedGame", new
            {
                RoomId = roomId,
                UserId = userId,
                Username = username,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to join game for user {UserId} in room {RoomId}", userId, roomId);
            await Clients.Caller.SendAsync("Error", "Failed to join game");
        }
    }

    public async Task LeaveGame(int roomId, int userId, string username)
    {
        try
        {
            var groupName = $"Game_{roomId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            _connectionToUser.Remove(Context.ConnectionId);
            _userToConnection.Remove(userId);

            await Clients.Group(groupName).SendAsync("PlayerLeftGame", new
            {
                RoomId = roomId,
                UserId = userId,
                Username = username,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("User {Username} left game in room {RoomId}", username, roomId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to leave game for user {UserId} in room {RoomId}", userId, roomId);
        }
    }

    public async Task StartGame(int roomId)
    {
        try
        {
            var success = await _gameService.StartGameAsync(roomId);
            if (!success)
            {
                await Clients.Caller.SendAsync("GameStartFailed", new
                {
                    Message = "Failed to start game",
                    RoomId = roomId
                });
                return;
            }

            var groupName = $"Game_{roomId}";
            var gameState = await _gameService.GetGameStateAsync(roomId);

            await Clients.Group(groupName).SendAsync("GameStarted", new
            {
                RoomId = roomId,
                GameState = gameState,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Game started in room {RoomId}", roomId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start game in room {RoomId}", roomId);
            await Clients.Caller.SendAsync("GameStartFailed", new
            {
                Message = "Failed to start game",
                Error = ex.Message
            });
        }
    }

    public async Task MakeMove(int roomId, int playerId, GameMoveDto move)
    {
        try
        {
            var isPlayerTurn = await _gameService.IsPlayerTurnAsync(roomId, playerId);
            if (!isPlayerTurn)
            {
                await Clients.Caller.SendAsync("MoveError", "It's not your turn");
                return;
            }

            var result = await _gameService.MakeMoveAsync(roomId, playerId, move);

            if (!result.Success)
            {
                await Clients.Caller.SendAsync("MoveError", result.Message);
                return;
            }

            var groupName = $"Game_{roomId}";

            await Clients.Group(groupName).SendAsync("MoveCompleted", new
            {
                RoomId = roomId,
                PlayerId = playerId,
                Move = move,
                Result = result,
                Timestamp = DateTime.UtcNow
            });

            if (result.GameEnded && result.WinnerId.HasValue)
            {
                await Clients.Group(groupName).SendAsync("GameEnded", new
                {
                    RoomId = roomId,
                    WinnerId = result.WinnerId.Value,
                    Message = result.Message,
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInformation("Game ended in room {RoomId}, winner: {WinnerId}",
                    roomId, result.WinnerId.Value);
            }

            _logger.LogInformation("Move completed by player {PlayerId} in room {RoomId}", playerId, roomId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process move by player {PlayerId} in room {RoomId}",
                playerId, roomId);
            await Clients.Caller.SendAsync("MoveError", "Failed to process move");
        }
    }

    public async Task BelieveOrNot(int roomId, int challengerId, bool believe)
    {
        try
        {
            var result = await _gameService.BelieveOrNotAsync(roomId, challengerId, believe);

            if (!result.Success)
            {
                await Clients.Caller.SendAsync("BelieveError", result.Message);
                return;
            }

            var groupName = $"Game_{roomId}";

            await Clients.Group(groupName).SendAsync("BelieveResult", new
            {
                RoomId = roomId,
                ChallengerId = challengerId,
                Believe = believe,
                Result = result,
                Timestamp = DateTime.UtcNow
            });

            if (result.GameEnded && result.WinnerId.HasValue)
            {
                await Clients.Group(groupName).SendAsync("GameEnded", new
                {
                    RoomId = roomId,
                    WinnerId = result.WinnerId.Value,
                    Message = "Game ended after challenge",
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInformation("Game ended in room {RoomId} after challenge, winner: {WinnerId}",
                    roomId, result.WinnerId.Value);
            }

            _logger.LogInformation("Believe challenge by player {ChallengerId} in room {RoomId}: {Believe}",
                challengerId, roomId, believe ? "believed" : "didn't believe");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process believe challenge by player {ChallengerId} in room {RoomId}",
                challengerId, roomId);
            await Clients.Caller.SendAsync("BelieveError", "Failed to process challenge");
        }
    }

    public async Task GetGameState(int roomId)
    {
        try
        {
            var gameState = await _gameService.GetGameStateAsync(roomId);
            if (gameState != null)
            {
                await Clients.Caller.SendAsync("GameStateUpdated", gameState);
            }
            else
            {
                await Clients.Caller.SendAsync("GameNotFound", new { RoomId = roomId });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get game state for room {RoomId}", roomId);
            await Clients.Caller.SendAsync("Error", "Failed to get game state");
        }
    }

    public async Task RefreshGameState(int roomId)
    {
        try
        {
            var groupName = $"Game_{roomId}";
            var gameState = await _gameService.GetGameStateAsync(roomId);

            if (gameState != null)
            {
                await Clients.Group(groupName).SendAsync("GameStateUpdate", gameState);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh game state for room {RoomId}", roomId);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connectionToUser.TryGetValue(Context.ConnectionId, out var userId))
        {
            _connectionToUser.Remove(Context.ConnectionId);
            _userToConnection.Remove(userId);

            await Clients.All.SendAsync("PlayerDisconnected", new
            {
                UserId = userId,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("User {UserId} disconnected from game", userId);
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