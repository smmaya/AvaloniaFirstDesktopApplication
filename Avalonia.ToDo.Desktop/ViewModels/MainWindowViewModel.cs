using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Shared.Services;
using Avalonia.ToDo.Desktop.Views;

namespace Avalonia.ToDo.Desktop.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly ToDoService _service;
    private UserControl _currentView = new();
    
    public required Window MainWindow { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public UserControl CurrentView
    {
        get => _currentView;
        set
        {
            if (_currentView != value)
            {
                _currentView = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentView)));
            }
        }
    }

    public MainWindowViewModel()
    {
        _service = new ToDoService(new HttpClient { BaseAddress = new Uri("http://localhost:5119/") });
        
        _ = InitializeAsync();
    }
    
    private async Task InitializeAsync()
    {
        try
        {
            var listViewModel = new ToDoListViewModel( this, _service);
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
        switch (viewModel)
        {
            case ToDoListViewModel list:
                CurrentView = new ToDoListView { DataContext = list };
                break;
            case ToDoEditorViewModel editor:
                CurrentView = new ToDoEditorView { DataContext = editor };
                break;
            case ToDoDetailsViewModel details:
                CurrentView = new ToDoDetailsView { DataContext = details };
                break;
        }
    }
}