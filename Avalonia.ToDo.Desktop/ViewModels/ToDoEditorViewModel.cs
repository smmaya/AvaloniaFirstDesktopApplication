using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Shared.Interfaces;
using Avalonia.Shared.ModelDtos;
using Avalonia.ToDo.Desktop.Helpers;

namespace Avalonia.ToDo.Desktop.ViewModels;

public class ToDoEditorViewModel : INotifyPropertyChanged
{
    private readonly MainWindowViewModel _main;
    private readonly IToDoService _service;
    private readonly bool _isEditMode;

    private ToDoDto Item { get; set; }
    public ICommand SaveCommand { get; set; }
    public ICommand CancelCommand { get; set; }
    
    public string TitleText => _isEditMode ? "Edit ToDo" : "Create ToDo";

    public event PropertyChangedEventHandler? PropertyChanged;

    public ToDoEditorViewModel(MainWindowViewModel main, IToDoService service, ToDoDto? existingItem = null)
    {
        _main = main;
        _service = service;

        if (existingItem != null && existingItem.Id != 0)
        {
            _isEditMode = true;
            Item = new ToDoDto
            {
                Id = existingItem.Id,
                Title = existingItem.Title,
                Description = existingItem.Description,
                IsCompleted = existingItem.IsCompleted
            };
        }
        else
        {
            Item = existingItem ?? new ToDoDto();
            _isEditMode = false;
        }

        SaveCommand = new RelayCommand(async _ => await SaveAsync());
        CancelCommand = new RelayCommand(_ => NavigateBack());
    }
    
    private async Task SaveAsync()
    {
        var title = Item.Title.Trim();
        var description = Item.Description.Trim();

        if (string.IsNullOrEmpty(title))
        {
            await ShowError("Title cannot be empty.");
            return;
        }

        if (string.IsNullOrEmpty(description))
        {
            await ShowError("Description cannot be empty.");
            return;
        }

        if (_isEditMode)
        {
            await _service.UpdateAsync(Item);
        }
        else
        {
            await _service.CreateAsync(Item);
        }

        NavigateBack();
    }
    
    private async Task ShowError(string message)
    {
        var window = new Controls.Window
        {
            Width = 350,
            Height = 180,
            CanResize = false,
            WindowStartupLocation = Controls.WindowStartupLocation.CenterOwner,
            Title = "Validation Error"
        };
        
        var stack = new Controls.StackPanel
        {
            Margin = new Thickness(12),
            Spacing = 20,
            HorizontalAlignment = Layout.HorizontalAlignment.Center,
            VerticalAlignment = Layout.VerticalAlignment.Center
        };
        
        var textBlock = new Controls.TextBlock
        {
            Text = message,
            TextWrapping = Media.TextWrapping.Wrap,
            HorizontalAlignment = Layout.HorizontalAlignment.Center,
            TextAlignment = Media.TextAlignment.Center
        };
        stack.Children.Add(textBlock);
        
        var okButton = new Controls.Button
        {
            Content = "OK",
            Width = 80,
            HorizontalAlignment = Layout.HorizontalAlignment.Center
        };
        okButton.Click += (_, _) => window.Close();
        stack.Children.Add(okButton);

        window.Content = stack;

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is { } mainWindow)
        {
            await window.ShowDialog(mainWindow);
        }
        else
        {
            window.Show();
        }
    }
    
    private void NavigateBack()
    {
        var list = new ToDoListViewModel(_main, _service);
        _ = list.LoadAsync();
        _main.NavigateTo(list);
    }
}