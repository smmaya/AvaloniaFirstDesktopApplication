using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Shared.Enums;
using Avalonia.Threading;

namespace Avalonia.ToDo.Desktop.ViewModels;

public class LogPanelViewModel : ViewModelBase
{
    private readonly HttpClient _client = new() { BaseAddress = new Uri("http://localhost:7000") };
    private readonly Timer _timer;

    private ObservableCollection<LogDto> Logs { get; } = new();

    public LogPanelViewModel()
    {
        _timer = new Timer(2000); // refresh every 2s
        _timer.Elapsed += async (_, _) => await LoadLogs();
        _timer.Start();
    }

    private async Task LoadLogs()
    {
        try
        {
            var logs = await _client.GetFromJsonAsync<List<LogDto>>("/logs");
            if (logs is null) return;

            Dispatcher.UIThread.Post(() =>
            {
                Logs.Clear();
                foreach (var l in logs)
                {
                    Logs.Add(new LogDto(l.Message, l.Timestamp, l.Type));
                }
            });
        }
        catch { /* ignore connection failures */ }
    }
    
    private record LogDto(string Message, DateTime Timestamp, LogType Type)
    {
        public string Display => $"{Timestamp:HH:mm:ss} - {Message}";
    };
}