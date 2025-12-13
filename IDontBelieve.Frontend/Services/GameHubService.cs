using System.Security.Claims;
using IDontBelieve.Core.DTOs.Frontend;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components.Authorization;

namespace IDontBelieve.Frontend.Services;

public class GameHubService : IAsyncDisposable
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    
    public readonly HubConnection? _hubConnection;

    public List<GameRoomDto> Rooms { get; private set; } = new();
    public GameRoomDto? CurrentRoom { get; private set; }
    public int CreatedRoomId { get; private set; }

    public event Action? OnRoomsUpdated;
    public event Action? OnCurrentRoomUpdated;

    public GameHubService(NavigationManager navManager, AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;
        
        var serverUrl = "http://localhost:5000/hubs/gameroom";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(serverUrl)
            .WithAutomaticReconnect()
            .Build();

        RegisterHubEvents();
    }

    private void RegisterHubEvents()
    {
        
        _hubConnection.On<List<IDontBelieve.Core.DTOs.Frontend.GameRoomDto>>("RoomsList", (rooms) =>
        {
            Console.WriteLine($"RoomsList");
            Rooms = rooms;
            OnRoomsUpdated?.Invoke();
        });

        _hubConnection.On<GameRoomDto>("RoomCreated", (room) =>
        {
                Console.WriteLine($"RoomCreated: {room}");
                CreatedRoomId =  room.Id;
                // Rooms.Add(room);
                // OnRoomsUpdated?.Invoke();
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
        
        OnRoomsUpdated += () =>
        {
            Console.WriteLine($"RoomsUpdated: {Rooms.Count}");
        };
        
        Console.WriteLine($"Finish inti");
        
        _hubConnection.Closed += async (error) =>
        {
            Console.WriteLine($"Connection closed: {error}");
            await Task.Delay(5000);
            await _hubConnection.StartAsync();
        };

        _hubConnection.Reconnecting += (error) =>
        {
            Console.WriteLine($"Reconnecting: {error}");
            return Task.CompletedTask;
        };
    }

    public async Task ConnectAsync()
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            Console.WriteLine("TryConnect");
            await _hubConnection.StartAsync();
            Console.WriteLine($"Status from service: {_hubConnection.State}");
        }
    }

    public async Task JoinLobbyAsync()
    {
        await ConnectAsync();
        await _hubConnection.InvokeAsync("JoinLobby", await GetUserId(), await GetUsername());
    }

    public async Task CreateRoomAsync(string roomName, int maxPlayers)
    {
        var dto = new IDontBelieve.Core.DTOs.CreateRoomDto 
            { Name = roomName, MaxPlayers = maxPlayers };
        
        var id = await GetUserId();
        await _hubConnection.InvokeAsync("CreateRoom", dto, id);
    }

    public async Task JoinRoomAsync(int roomId)
    {
        await _hubConnection.InvokeAsync("JoinRoom", roomId, await GetUserId(), await GetUsername());
        // Explicitly ask for details to populate CurrentRoom immediately
        await _hubConnection.InvokeAsync("GetRoomDetails", roomId);
    }

    public async Task LeaveRoomAsync(int roomId)
    {
        await _hubConnection.InvokeAsync("LeaveRoom", roomId, await GetUserId(), await GetUsername());
        CurrentRoom = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }

    public async Task<int> GetUserId()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User; 
        return int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }
    
    public async Task<string> GetUsername()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User; 
        return user.FindFirst(ClaimTypes.Name)?.Value;
    }
}