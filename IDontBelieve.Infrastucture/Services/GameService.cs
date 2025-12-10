
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IDontBelieve.Core.Models;
using IDontBelieve.Core.Services;
using IDontBelieve.Core.DTOs;
using IDontBelieve.Core.Game;
using IDontBelieve.Infrastructure.Data;

namespace IDontBelieve.Infrastructure.Services;

public class GameService : IGameService
{
    //private readonly ApplicationDbContext _context;
    private readonly IGameRoomService _gameRoomService;
    private readonly ILogger<GameService> _logger;

    private readonly List<GameState> _gameStates = new();
    private readonly List<GameMove> _gameMoves = new();

    //public GameService(ApplicationDbContext context, IGameRoomService gameRoomService, ILogger<GameService> logger)
    public GameService(IGameRoomService gameRoomService, ILogger<GameService> logger)

    {
        //_context = context;
        _gameRoomService = gameRoomService;
        _logger = logger;
    }

    public async Task<bool> StartGameAsync(int roomId)
    {
        /*var room = await _context.GameRooms
            .Include(r => r.Players)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(r => r.Id == roomId);*/
        
        var room = _gameRoomService.GetRoomByIdAsync(roomId);

        if (room == null || !room.CanStart || room.Status != GameRoomStatus.Waiting)
        {
            return false;
        }

        var deck = CardDeck.CreateDeck(room.DeckType);
        var shuffledDeck = CardDeck.Shuffle(deck);
        var hands = CardDeck.DealCards(shuffledDeck, room.Players.Count);

        var playersList = room.Players.OrderBy(p => p.Position).ToList();
        for (int i = 0; i < playersList.Count; i++)
        {
            playersList[i].Hand = hands[i];
            playersList[i].Status = PlayerStatus.Playing;
        }

        var gameState = new GameState
        {
            GameRoomId = roomId,
            CurrentPlayerId = playersList[0].UserId, 
            Deck = new List<Card>(), 
            DiscardPile = new List<Card>(),
            GameBank = 0,
            Phase = GamePhase.Playing,
            LastMoveAt = DateTime.UtcNow
        };

        //_context.GameStates.Add(gameState);
        
        room.Status = GameRoomStatus.InProgress;

        //await _context.SaveChangesAsync();

        _logger.LogInformation("Game started in room {RoomId} with {PlayerCount} players", 
            roomId, room.Players.Count);
        
        return true;
    }

    public async Task<GameStateDto?> GetGameStateAsync(int roomId)
    {
        /*var gameState = await _context.GameStates
            .Include(gs => gs.GameRoom)
            .ThenInclude(gr => gr.Players)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(gs => gs.GameRoomId == roomId);*/
        
        var gameState = _gameStates.FirstOrDefault(gs => gs.GameRoomId == roomId);

        if (gameState == null) return null;

        var players = gameState.GameRoom.Players.Select(p => new PlayerStateDto
        {
            UserId = p.UserId,
            UserName = p.User.UserName,
            CardCount = p.Hand.Count,
            Status = p.Status,
            Position = p.Position,
            Hand = null 
        }).ToList();

        return new GameStateDto
        {
            CurrentPlayerId = gameState.CurrentPlayerId,
            CardsInDeck = gameState.Deck.Count,
            CardsInDiscard = gameState.DiscardPile.Count,
            Players = players,
            Phase = gameState.Phase,
            GameBank = gameState.GameBank,
            LastMoveAt = gameState.LastMoveAt
        };
    }

    public async Task<GameMoveResultDto> MakeMoveAsync(int roomId, int playerId, GameMoveDto move)
    {
        var gameState = await GetGameStateWithPlayersAsync(roomId);
        if (gameState == null)
        {
            return new GameMoveResultDto
            {
                Success = false,
                Message = "Game not found"
            };
        }

        if (gameState.CurrentPlayerId != playerId)
        {
            return new GameMoveResultDto
            {
                Success = false,
                Message = "Not your move"
            };
        }

        var player = gameState.GameRoom.Players.FirstOrDefault(p => p.UserId == playerId);
        if (player == null)
        {
            return new GameMoveResultDto
            {
                Success = false,
                Message = "Player not found"
            };
        }

        if (!GameLogic.IsValidMove(player.Hand, move.Cards, move.ClaimedRank))
        {
            return new GameMoveResultDto
            {
                Success = false,
                Message = "Invalid move"
            };
        }

        foreach (var card in move.Cards)
        {
            player.Hand.Remove(card);
        }

        var discardPile = gameState.DiscardPile;
        discardPile.AddRange(move.Cards);
        gameState.DiscardPile = discardPile;

        var gameMove = new GameMove
        {
            GameStateId = gameState.Id,
            MoveNumber = gameState.MoveHistory.Count + 1,
            PlayerId = playerId,
            TargetPlayerId = move.TargetPlayerId,
            CardsPlayed = move.Cards,
            Outcome = MoveOutcome.Continue,
            CreatedAt = DateTime.UtcNow
        };

        _gameMoves.Add(gameMove);
        //_context.GameMoves.Add(gameMove);

        if (GameLogic.IsGameEnded(gameState.GameRoom.Players.ToList()))
        {
            var winnerId = GameLogic.GetWinnerId(gameState.GameRoom.Players.ToList());
            await EndGameAsync(roomId, winnerId);
            
            return new GameMoveResultDto
            {
                Success = true,
                Message = "Game is over",
                GameEnded = true,
                WinnerId = winnerId,
                NewState = await GetGameStateAsync(roomId)
            };
        }

        gameState.CurrentPlayerId = GameLogic.GetNextPlayerId(
            gameState.GameRoom.Players.ToList(), 
            gameState.CurrentPlayerId);
        gameState.LastMoveAt = DateTime.UtcNow;

        //await _context.SaveChangesAsync();

        return new GameMoveResultDto
        {
            Success = true,
            Message = "Move is made",
            NewState = await GetGameStateAsync(roomId)
        };
    }

    public async Task<GameMoveResultDto> BelieveOrNotAsync(int roomId, int playerId, bool believe)
    {
        var gameState = await GetGameStateWithPlayersAsync(roomId);
        if (gameState == null)
        {
            return new GameMoveResultDto
            {
                Success = false,
                Message = "Game not found"
            };
        }

        var lastMove = gameState.MoveHistory.OrderByDescending(m => m.MoveNumber).FirstOrDefault();
        if (lastMove == null)
        {
            return new GameMoveResultDto
            {
                Success = false,
                Message = "No moves to check"
            };
        }

        if (believe)
        {
            lastMove.Outcome = MoveOutcome.Believe;
        }
        else
        {
            // make after
            lastMove.Outcome = MoveOutcome.NotBelieve;
        }

        //await _context.SaveChangesAsync();

        return new GameMoveResultDto
        {
            Success = true,
            Message = believe ? "Поверили" : "Не поверили",
            NewState = await GetGameStateAsync(roomId)
        };
    }

    public async Task EndGameAsync(int roomId, int winnerId)
    {
        /*var gameState = await _context.GameStates
            .Include(gs => gs.GameRoom)
            .ThenInclude(gr => gr.Players)
            .FirstOrDefaultAsync(gs => gs.GameRoomId == roomId);*/
        
        var gameState = _gameStates.FirstOrDefault(g => g.GameRoomId == roomId);

        if (gameState == null) return;

        gameState.Phase = GamePhase.Finished;
        gameState.GameRoom.Status = GameRoomStatus.Finished;

        foreach (var player in gameState.GameRoom.Players)
        {
            if (player.UserId == winnerId)
            {
                player.Status = PlayerStatus.Winner;
            }
            else
            {
                player.Status = PlayerStatus.Eliminated;
            }
        }

        //await _context.SaveChangesAsync();

        _logger.LogInformation("Game ended in room {RoomId}, winner: {WinnerId}", roomId, winnerId);
    }

    public async Task<bool> IsValidMoveAsync(int roomId, int playerId, GameMoveDto move)
    {
        var gameState = await GetGameStateWithPlayersAsync(roomId);
        if (gameState == null) return false;

        var player = gameState.GameRoom.Players.FirstOrDefault(p => p.UserId == playerId);
        if (player == null) return false;

        return GameLogic.IsValidMove(player.Hand, move.Cards, move.ClaimedRank);
    }

    public async Task<bool> IsPlayerTurnAsync(int roomId, int playerId)
    {
        /*var gameState = await _context.GameStates
            .FirstOrDefaultAsync(gs => gs.GameRoomId == roomId);*/
        
        var gameState = _gameStates.FirstOrDefault(gs => gs.GameRoomId == roomId);

        return gameState?.CurrentPlayerId == playerId;
    }

    private async Task<GameState?> GetGameStateWithPlayersAsync(int roomId)
    {
        /*return await _context.GameStates
            .Include(gs => gs.GameRoom)
            .ThenInclude(gr => gr.Players)
            .ThenInclude(p => p.User)
            .Include(gs => gs.MoveHistory)
            .FirstOrDefaultAsync(gs => gs.GameRoomId == roomId);*/
        
        return _gameStates.FirstOrDefault(gs => gs.GameRoomId == roomId);
    }
}


