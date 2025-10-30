using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Shared.ModelDtos;
using Avalonia.Shared.Services;
using Avalonia.ToDo.Desktop.Models;
using Avalonia.ToDo.Desktop.Views;
using CommunityToolkit.Mvvm.Input;
using RelayCommand = Avalonia.ToDo.Desktop.Helpers.RelayCommand;

namespace Avalonia.ToDo.Desktop.ViewModels;

public class ToDoListViewModel
{
    private readonly ToDoService _service;
    private readonly MainWindowViewModel? _main;
    public ObservableCollection<ToDoDto> ToDos { get; } = new();
    public ICommand CreateCommand { get; set; }
    public ICommand RefreshCommand { get; set; }
    public ICommand ViewCommand { get; set; }
    public ICommand EditCommand { get; set; }
    public ICommand DeleteCommand { get; set; }

    public ToDoListViewModel(MainWindowViewModel? main, ToDoService service)
    {
        _service = service;
        _main = main;
        
        CreateCommand = new RelayCommand(async _ => await CreateAsync());
        RefreshCommand = new RelayCommand(async _ => await LoadAsync());
        ViewCommand = new RelayCommand(param => ViewItem(param as ToDoDto), p => p is ToDoDto);
        EditCommand = new RelayCommand(param => EditItem(param as ToDoDto), p => p is ToDoDto);
        DeleteCommand = new AsyncRelayCommand<ToDoDto>(async item =>
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
                ToDos.Add(todo);
            }
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
        }
    }

    private Task CreateAsync()
    {
        if (_main is null)
        {
            return Task.CompletedTask;
        }

        var newItem = new ToDoDto();
        
        _main.NavigateTo(new ToDoEditorViewModel(_main, _service, newItem));
        
        return Task.CompletedTask;
    }

    private void ViewItem(ToDoDto? item)
    {
        if (item == null || _main is null)
        {
            return;
        }

        _main.NavigateTo(new ToDoDetailsViewModel(_main, _service, item));
    }

    private void EditItem(ToDoDto? item)
    {
        if (item == null || _main is null)
        {
            return;
        }

        _main.NavigateTo(new ToDoEditorViewModel(_main, _service, item));
    }

    private async Task DeleteItemAsync(ToDoDto item)
    {
        if (_main == null)
        {
            return;
        }

        var dialog = new DeleteConfirmationWindow();
        var result = await dialog.ShowDialog<bool?>(_main.MainWindow);
        
        if (result == true)
        {
            await _service.DeleteAsync(item.Id);
            ToDos.Remove(item);
        }
    }
}