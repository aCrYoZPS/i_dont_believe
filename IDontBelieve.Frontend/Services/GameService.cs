using IDontBelieve.Core.DTOs;
using Microsoft.AspNetCore.SignalR.Client;

namespace IDontBelieve.Frontend.Services;

public class GameService : IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    private readonly string _apiBaseUrl;

    public GameStateDto? CurrentState { get; private set; }

    public event Action? OnGameStateUpdated;
    public event Action<string>? OnGameError;
    public event Action<int>? OnGameEnded;

    public GameService(string apiBaseUrl)
    {
        _apiBaseUrl = apiBaseUrl.TrimEnd('/');
        var gameHubUrl = $"{_apiBaseUrl}/hubs/game";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(gameHubUrl)
            .WithAutomaticReconnect()
            .Build();

        RegisterHubEvents(_hubConnection);
    }

    private void RegisterHubEvents(HubConnection hub)
    {
        hub.On<GameStateDto>("GameStarted", state =>
        {
            CurrentState = state;
            OnGameStateUpdated?.Invoke();
        });

        hub.On<GameStateDto>("GameStateUpdate", state =>
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

    private async Task EnsureConnectedAsync()
    {
        if (_hubConnection.State == HubConnectionState.Connected)
            return;

        // если был reconnecting/connecting — дождёмся
        if (_hubConnection.State == HubConnectionState.Reconnecting ||
            _hubConnection.State == HubConnectionState.Connecting)
        {
            await Task.Delay(200);
        }

        if (_hubConnection.State != HubConnectionState.Connected)
        {
            await _hubConnection.StartAsync();
        }
    }

    // ----------------------------
    // Commands
    // ----------------------------

    public async Task JoinGameAsync(int roomId, int userId, string username)
    {
        await EnsureConnectedAsync();
        await _hubConnection.InvokeAsync("JoinGame", roomId, userId, username);
    }

    public async Task StartGameAsync(int roomId)
    {
        await EnsureConnectedAsync();
        await _hubConnection.InvokeAsync("StartGame", roomId);
    }

    public async Task LoadGameStateAsync(int roomId)
    {
        await EnsureConnectedAsync();
        CurrentState = await _hubConnection
            .InvokeAsync<GameStateDto>("GetGameState", roomId);

        OnGameStateUpdated?.Invoke();
    }

    public async Task MakeMoveAsync(int roomId, int playerId, GameMoveDto move)
    {
        await EnsureConnectedAsync();
        await _hubConnection.InvokeAsync("MakeMove", roomId, playerId, move);
    }

    public async Task BelieveOrNotAsync(int roomId, int playerId, bool believe)
    {
        await EnsureConnectedAsync();
        await _hubConnection.InvokeAsync("BelieveOrNot", roomId, playerId, believe);
    }

    public bool IsMyTurn(int myUserId)
    {
        return CurrentState?.CurrentPlayerId == myUserId;
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection.State != HubConnectionState.Disconnected)
        {
            await _hubConnection.StopAsync();
        }
        await _hubConnection.DisposeAsync();
        CurrentState = null;
    }
}
