using Avalonia.Shared.Enums;
using Avalonia.Shared.Interfaces;
using Avalonia.Shared.Records;

namespace Avalonia.Api.Services;

public class RemoteLogger(HttpClient client) : IRemoteLogger
{
    public async Task LogAsync(string message, LogType type = LogType.Info)
    {
        var entry = new LogEntry(message, DateTime.UtcNow, type);
        
        await client.PostAsJsonAsync("/logs", entry);
    }
}