using System;
using System.Threading.Tasks;
using Avalonia.Shared.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;

namespace Avalonia.ToDo.Desktop.Services;

public class SignalRService
{
    private readonly IAuthService _authService;
    private HubConnection? _connection;

    public SignalRService(IAuthService authService)
    {
        _authService = authService;
    }

    public event Action<string, int>? OnToDoUpdated;
    
    public async Task StartAsync()
    {
        if (_connection != null) return;
        if (string.IsNullOrEmpty(_authService.Token))
            throw new InvalidOperationException("JWT token missing");

        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5119/hubs/todo", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_authService?.Token);
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<string, int>("ToDoUpdated", (action, id) =>
        {
            Console.WriteLine($"[SignalR] Received notification: {action} for ID: {id}");
            OnToDoUpdated?.Invoke(action, id);
        });

        try
        {
            Console.WriteLine("[SignalR] Connecting to hub...");
            await _connection.StartAsync();
            Console.WriteLine("[SignalR] Connected successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Error starting connection: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[SignalR] Inner error: {ex.InnerException.Message}");
            }
        }
    }

    public async Task StopAsync()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}
