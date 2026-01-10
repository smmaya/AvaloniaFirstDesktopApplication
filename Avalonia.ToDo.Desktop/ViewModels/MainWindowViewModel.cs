using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Shared.Interfaces;
using Avalonia.ToDo.Desktop.Views;

using Avalonia.ToDo.Desktop.Services;

namespace Avalonia.ToDo.Desktop.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly IToDoService _service;
    private readonly IRemoteLogger _remoteLogger;
    private readonly SignalRService _signalRService;
    private UserControl _currentView = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public UserControl CurrentView
    {
        get => _currentView;
        set
        {
            _currentView = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentView)));
        }
    }

    public MainWindowViewModel(IToDoService service, IRemoteLogger remoteLogger, SignalRService signalRService)
    {
        _service = service;
        _remoteLogger = remoteLogger;
        _signalRService = signalRService;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            await _signalRService.StartAsync();

            var listViewModel = new ToDoListViewModel(this, _service, _remoteLogger, _signalRService);
            await listViewModel.LoadAsync();
            CurrentView = new ToDoListView { DataContext = listViewModel };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize: {ex.Message}");
        }
    }

    public void NavigateTo(object viewModel)
    {
        CurrentView = viewModel switch
        {
            ToDoListViewModel list => new ToDoListView { DataContext = list },
            ToDoEditorViewModel editor => new ToDoEditorView { DataContext = editor },
            ToDoDetailsViewModel details => new ToDoDetailsView { DataContext = details },
            _ => CurrentView
        };
    }
}