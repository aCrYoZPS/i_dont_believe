using IDontBelieve.Core.DTOs;
using Microsoft.AspNetCore.SignalR.Client;

namespace IDontBelieve.Frontend.Services;

public class GameService : IAsyncDisposable
{
    private readonly GameHubService _hubService;

    public GameStateDto? CurrentState { get; private set; }

    public event Action? OnGameStateUpdated;
    public event Action<string>? OnGameError;
    public event Action<int>? OnGameEnded;

    public GameService(GameHubService hubService)
    {
        _hubService = hubService;

        if (_hubService._hubConnection != null)
        {
            RegisterHubEvents(_hubService._hubConnection);
        }
    }

    private void RegisterHubEvents(HubConnection hub)
    {
        hub.On<GameStateDto>("GameStarted", state =>
        {
            CurrentState = state;
            OnGameStateUpdated?.Invoke();
        });

        hub.On<GameStateDto>("GameStateUpdated", state =>
        {
            CurrentState = state;
            OnGameStateUpdated?.Invoke();
        });

        hub.On<int>("GameEnded", winnerId =>
        {
            OnGameEnded?.Invoke(winnerId);
        });

        hub.On<string>("MoveRejected", message =>
        {
            OnGameError?.Invoke(message);
        });
    }

    // ----------------------------
    // Commands
    // ----------------------------

    public async Task StartGameAsync(int roomId)
    {
        await _hubService.ConnectAsync();
        await _hubService._hubConnection!.InvokeAsync("StartGame", roomId);
    }

    public async Task LoadGameStateAsync(int roomId)
    {
        await _hubService.ConnectAsync();
        CurrentState = await _hubService._hubConnection!
            .InvokeAsync<GameStateDto>("GetGameStateAsync", roomId);

        OnGameStateUpdated?.Invoke();
    }

    public async Task MakeMoveAsync(int roomId, int playerId, GameMoveDto move)
    {
        await _hubService._hubConnection!
            .InvokeAsync("MakeMove", roomId, playerId, move);
    }

    public async Task BelieveOrNotAsync(int roomId, int playerId, bool believe)
    {
        await _hubService._hubConnection!
            .InvokeAsync("BelieveOrNot", roomId, playerId, believe);
    }

    public bool IsMyTurn(int myUserId)
    {
        return CurrentState?.CurrentPlayerId == myUserId;
    }

    public async ValueTask DisposeAsync()
    {
        CurrentState = null;
    }
}
