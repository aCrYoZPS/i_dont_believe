using System.Net.Http.Headers;
using IDontBelieve.Core.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace IDontBelieve.Frontend.Services;

public class LeaderboardHubService : ILeaderboardHubService, IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly IJSRuntime _jsRuntime;
    private readonly string _apiBaseUrl;

    public event Action<LeaderboardUpdate>? OnLeaderboardUpdated;
    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public LeaderboardHubService(IJSRuntime jsRuntime, string apiBaseUrl)
    {
        _jsRuntime = jsRuntime;
        _apiBaseUrl = apiBaseUrl;
    }

    public async Task ConnectAsync()
    {
        if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
            return;

        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{_apiBaseUrl}/hubs/leaderboard", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
            })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<LeaderboardUpdate>("LeaderboardUpdated", update =>
        {
            OnLeaderboardUpdated?.Invoke(update);
        });

        _hubConnection.Reconnecting += ex =>
        {
            Console.WriteLine($"SignalR reconnecting: {ex?.Message}");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += connectionId =>
        {
            Console.WriteLine($"SignalR reconnected: {connectionId}");
            return Task.CompletedTask;
        };

        _hubConnection.Closed += ex =>
        {
            Console.WriteLine($"SignalR closed: {ex?.Message}");
            return Task.CompletedTask;
        };

        await _hubConnection.StartAsync();
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }

    public async Task<LeaderboardUpdate> GetLeaderboardAsync(int count = 10)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
            await ConnectAsync();

        return await _hubConnection!.InvokeAsync<LeaderboardUpdate>("GetLeaderboard", count);
    }

    public async Task SubscribeToUpdatesAsync()
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
            await ConnectAsync();

        await _hubConnection!.InvokeAsync("SubscribeToLeaderboard");
    }

    public async Task UnsubscribeFromUpdatesAsync()
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("UnsubscribeFromLeaderboard");
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}