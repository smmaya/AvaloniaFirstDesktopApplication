using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Avalonia.Api.Hubs;

[Authorize]
public class ToDoHub : Hub
{
    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"SignalR connected: {Context.User?.Identity?.Name}");
        return base.OnConnectedAsync();
    }
}
