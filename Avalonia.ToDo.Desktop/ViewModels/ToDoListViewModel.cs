using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Shared.Enums;
using Avalonia.Shared.Interfaces;
using Avalonia.ToDo.Desktop.Models;
using Avalonia.ToDo.Desktop.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RelayCommand = Avalonia.ToDo.Desktop.Helpers.RelayCommand;

using Avalonia.ToDo.Desktop.Services;
using Avalonia.Threading;

namespace Avalonia.ToDo.Desktop.ViewModels;

public class ToDoListViewModel : ObservableObject
{
    private readonly IToDoService _service;
    private readonly MainWindowViewModel? _main;
    private readonly IRemoteLogger _remoteLogger;
    private readonly SignalRService? _signalRService;

    private int _totalCount;
    private int _remainingCount;
    private bool _canCreate;

    private ObservableCollection<ToDoDesktopDto> ToDos { get; } = [];

    public int TotalCount
    {
        get => _totalCount;
        private set => SetProperty(ref _totalCount, value);
    }

    public int RemainingCount
    {
        get => _remainingCount;
        private set => SetProperty(ref _remainingCount, value);
    }
    
    public bool CanCreate
    {
        get => _canCreate;
        private set => SetProperty(ref _canCreate, value);
    }

    public ICommand CreateCommand { get; set; }
    public ICommand RefreshCommand { get; set; }
    public ICommand ViewCommand { get; set; }
    public ICommand EditCommand { get; set; }
    public ICommand DeleteCommand { get; set; }
    public ICommand RowDoubleTappedCommand { get; set; }

    public ToDoListViewModel(MainWindowViewModel? main, IToDoService service, IRemoteLogger remoteLogger, SignalRService? signalRService = null)
    {
        _service = service;
        _remoteLogger = remoteLogger;
        _main = main;
        _signalRService = signalRService;
        _canCreate = false;

        if (_signalRService != null)
        {
            _signalRService.OnToDoUpdated += HandleToDoUpdated;
        }

        CreateCommand = new RelayCommand(async _ => await CreateAsync());
        RefreshCommand = new RelayCommand(async _ => await LoadAsync());
        ViewCommand = new RelayCommand(param => ViewItem(param as ToDoDesktopDto), p => p is ToDoDesktopDto);
        EditCommand = new RelayCommand(param => EditItem(param as ToDoDesktopDto), p => p is ToDoDesktopDto);
        DeleteCommand = new AsyncRelayCommand<ToDoDesktopDto>(async item =>
        {
            if (item != null)
            {
                await DeleteItemAsync(item);
            }
        });
        RowDoubleTappedCommand = new RelayCommand(param => ViewItem(param as ToDoDesktopDto));
    }

    private void HandleToDoUpdated(string action, int id)
    {
        Dispatcher.UIThread.InvokeAsync(async () => await LoadAsync());
    }

    public async Task LoadAsync()
    {
        ToDos.Clear();

        try
        {
            var todos = await _service.GetAllAsync();

            foreach (var todo in todos)
            {
                ToDos.Add(new ToDoDesktopDto
                {
                    Id = todo.Id,
                    Title = todo.Title,
                    Description = todo.Description,
                    IsCompleted = todo.IsCompleted,
                    CreatedAt = todo.CreatedAt
                });
            }
            
            CanCreate = true;
        }
        catch (Exception)
        {
            ToDos.Add(new ToDoDesktopDto
            {
                CreatedAt = DateTime.UtcNow,
                Title = "⚠ No connection to the database",
                Description = "Check your network or API availability.",
                IsPlaceholder = true
            });
            
            CanCreate = false;
        }

        UpdateCounts();
    }

    private void UpdateCounts()
    {
        TotalCount = ToDos.Count(t => !t.IsPlaceholder);
        RemainingCount = ToDos.Count(t => t is { IsCompleted: false, IsPlaceholder: false });
    }

    private Task CreateAsync()
    {
        if (_main == null)
        {
            return Task.CompletedTask;
        }

        // throw new Exception("Triggered Create button error for demo purpose.");
        
        var newItem = new ToDoDesktopDto();
        _main.NavigateTo(new ToDoEditorViewModel(_main, _service, _remoteLogger, newItem));
        return Task.CompletedTask;
    }

    private void ViewItem(ToDoDesktopDto? item)
    {
        if (item == null || _main == null)
        {
            return;
        }

        _remoteLogger.LogAsync($"[ACTION] Viewing task Id: {item.Id}", LogType.View);
        
        _main.NavigateTo(new ToDoDetailsViewModel(_main, _service, _remoteLogger, item));
    }

    private void EditItem(ToDoDesktopDto? item)
    {
        if (item == null || _main == null)
        {
            return;
        }

        _main.NavigateTo(new ToDoEditorViewModel(_main, _service, _remoteLogger, item));
    }

    private async Task DeleteItemAsync(ToDoDesktopDto item)
    {
        var lifetime = Application.Current?.ApplicationLifetime
            as Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;

        if (lifetime?.MainWindow == null)
            return;

        var dialog = new DeleteConfirmationWindow();
        var result = await dialog.ShowDialog<bool?>(lifetime.MainWindow);

        if (result == true)
        {
            await _service.DeleteAsync(item.Id);
            ToDos.Remove(item);
            UpdateCounts();
        }
    }
}
