using Avalonia.Shared.Enums;

namespace Avalonia.Shared.Interfaces;

public interface IRemoteLogger
{
    Task LogAsync(string message, LogType type = LogType.Info);
}