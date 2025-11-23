using Avalonia.Shared.Enums;

namespace Avalonia.Shared.Records;

public record LogEntry(string Message, DateTime Timestamp, LogType Type = LogType.Info);
