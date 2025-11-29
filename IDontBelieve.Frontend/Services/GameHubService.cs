using IDontBelieve.Frontend.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace IDontBelieve.Frontend.Services;

public class GameHubService : IAsyncDisposable
{
    private readonly HubConnection? _hubConnection;

    public List<GameRoomDto> Rooms { get; private set; } = new();
    public GameRoomDto? CurrentRoom { get; private set; }

    public event Action? OnRoomsUpdated;
    public event Action? OnCurrentRoomUpdated;

    // MOCK, Implement actual auth
    public int MyUserId { get; private set; } = new Random().Next(1, 10000);
    public string MyUsername { get; private set; } = "Player" + new Random().Next(1, 1000);

    public GameHubService(NavigationManager navManager)
    {
        var serverUrl = "http://localhost:5107/gameroomhub";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(serverUrl)
            .WithAutomaticReconnect()
            .Build();

        RegisterHubEvents();
    }

    private void RegisterHubEvents()
    {
        _hubConnection.On<List<GameRoomDto>>("RoomsList", (rooms) =>
        {
            Rooms = rooms;
            OnRoomsUpdated?.Invoke();
        });

        _hubConnection.On<GameRoomDto>("RoomCreated", (room) =>
        {
            Rooms.Add(room);
            OnRoomsUpdated?.Invoke();
        });

        _hubConnection.On<GameRoomDto>("RoomUpdated", (room) =>
        {
            var index = Rooms.FindIndex(r => r.Id == room.Id);
            if (index != -1) Rooms[index] = room;

            // If we are in this room, update details
            if (CurrentRoom?.Id == room.Id)
            {
                CurrentRoom = room;
                OnCurrentRoomUpdated?.Invoke();
            }

            OnRoomsUpdated?.Invoke();
        });

        _hubConnection.On<object>("RoomDeleted", (data) =>
        {
            // Parse dynamic object or create a specific DTO for deletion
            // logic to remove room from Rooms list
            OnRoomsUpdated?.Invoke();
        });

        // --- Room/Player Events ---
        _hubConnection.On<object>("PlayerJoinedRoom", (data) =>
        {
            // Note: In a real app, deserialize 'data' to a concrete class to get the updated Room object
            // For now, we rely on the subsequent RoomUpdated call or re-fetch
        });

        _hubConnection.On<object>("JoinRoomSuccess", (data) =>
        {
            // Simple mapping for demo. Real app should use System.Text.Json to deserialize 'data'
            // assuming data.Room is available
        });

        _hubConnection.On<GameRoomDto>("RoomDetails", (room) =>
        {
            CurrentRoom = room;
            OnCurrentRoomUpdated?.Invoke();
        });
    }

    public async Task ConnectAsync()
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            await _hubConnection.StartAsync();
        }
    }

    public async Task JoinLobbyAsync()
    {
        await ConnectAsync();
        await _hubConnection.InvokeAsync("JoinLobby", MyUserId, MyUsername);
    }

    public async Task CreateRoomAsync(string roomName, int maxPlayers)
    {
        var dto = new CreateRoomDto { Name = roomName, MaxPlayers = maxPlayers };
        await _hubConnection.InvokeAsync("CreateRoom", dto, MyUserId);
    }

    public async Task JoinRoomAsync(int roomId)
    {
        await _hubConnection.InvokeAsync("JoinRoom", roomId, MyUserId, MyUsername);
        // Explicitly ask for details to populate CurrentRoom immediately
        await _hubConnection.InvokeAsync("GetRoomDetails", roomId);
    }

    public async Task LeaveRoomAsync(int roomId)
    {
        await _hubConnection.InvokeAsync("LeaveRoom", roomId, MyUserId, MyUsername);
        CurrentRoom = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}