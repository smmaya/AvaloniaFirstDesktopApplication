using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Shared.Interfaces;
using Avalonia.ToDo.Desktop.Views;

namespace Avalonia.ToDo.Desktop.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public Window? MainWindow { get; set; }
    private readonly IToDoService _service;
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

    public MainWindowViewModel(IToDoService service)
    {
        _service = service;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            var listViewModel = new ToDoListViewModel(this, _service);
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