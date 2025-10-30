using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Shared.Services;
using Avalonia.ToDo.Desktop.Models;
using Avalonia.ToDo.Desktop.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RelayCommand = Avalonia.ToDo.Desktop.Helpers.RelayCommand;

namespace Avalonia.ToDo.Desktop.ViewModels;

public class ToDoListViewModel : ObservableObject
{
    private readonly ToDoService _service;
    private readonly MainWindowViewModel? _main;

    private int _totalCount;
    private int _remainingCount;
    private bool _canCreate;

    public ObservableCollection<ToDoDesktopDto> ToDos { get; } = new();

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

    public ICommand CreateCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ViewCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public ToDoListViewModel(MainWindowViewModel? main, ToDoService service)
    {
        _service = service;
        _main = main;
        _canCreate = false;

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
        RemainingCount = ToDos.Count(t => !t.IsCompleted && !t.IsPlaceholder);
    }

    private Task CreateAsync()
    {
        if (_main == null)
        {
            return Task.CompletedTask;
        }

        var newItem = new ToDoDesktopDto();
        _main.NavigateTo(new ToDoEditorViewModel(_main, _service, newItem));
        return Task.CompletedTask;
    }

    private void ViewItem(ToDoDesktopDto? item)
    {
        if (item == null || _main == null)
        {
            return;
        }

        _main.NavigateTo(new ToDoDetailsViewModel(_main, _service, item));
    }

    private void EditItem(ToDoDesktopDto? item)
    {
        if (item == null || _main == null)
        {
            return;
        }

        _main.NavigateTo(new ToDoEditorViewModel(_main, _service, item));
    }

    private async Task DeleteItemAsync(ToDoDesktopDto item)
    {
        if (_main == null)
        {
            return;
        }

        var dialog = new DeleteConfirmationWindow();
        bool? result = await dialog.ShowDialog<bool?>(_main.MainWindow);

        if (result == true)
        {
            await _service.DeleteAsync(item.Id);
            ToDos.Remove(item);
            UpdateCounts();
        }
    }
}
