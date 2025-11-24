using System.Collections.Concurrent;
using Avalonia.Shared.Records;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(LogStore.Instance);
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

var app = builder.Build();

// Create log
app.MapPost("/logs", (LogEntry log, LogStore store) =>
{
    var logWithTimestamp = log with { Timestamp = DateTime.UtcNow };
    store.Logs.Add(logWithTimestamp);
    return Results.Created("/logs", logWithTimestamp);
});

// Get all logs
app.MapGet("/logs", (LogStore store) =>
    store.Logs.OrderByDescending(l => l.Timestamp));

app.Run();

public class LogStore
{
    public static readonly LogStore Instance = new();
    public ConcurrentBag<LogEntry> Logs { get; } = new();
}