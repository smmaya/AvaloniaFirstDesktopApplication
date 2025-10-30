using System.ComponentModel;
using System.Windows.Input;
using Avalonia.Shared.ModelDtos;
using Avalonia.Shared.Services;
using Avalonia.ToDo.Desktop.Helpers;

namespace Avalonia.ToDo.Desktop.ViewModels;

public class ToDoDetailsViewModel : INotifyPropertyChanged
{
    private readonly MainWindowViewModel _main;
    private readonly ToDoService _service;

    private ToDoDto? _item;
    public ToDoDto? Item
    {
        get => _item;
        set
        {
            if (_item != value)
            {
                _item = value;
                OnPropertyChanged(nameof(Item));
            }
        }
    }
    
    private bool _isEditing;
    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            if (_isEditing != value)
            {
                _isEditing = value;
                OnPropertyChanged(nameof(IsEditing));
            }
        }
    }

    public ICommand BackCommand { get; set; }
    public ICommand EditCommand { get; set; }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public ToDoDetailsViewModel(MainWindowViewModel main, ToDoService service, ToDoDto? existingItem = null)
    {
        _main = main;
        _service = service;

        Item = existingItem;
        IsEditing = false;

        BackCommand = new RelayCommand(_ => NavigateBack());
        EditCommand = new RelayCommand(_ => NavigateToEdit());
    }

    private void NavigateBack()
    {
        var list = new ToDoListViewModel(_main, _service);
        _ = list.LoadAsync();
        _main.NavigateTo(list);
    }

    private void NavigateToEdit()
    {
        var editor = new ToDoEditorViewModel(_main, _service, Item);
        _main.NavigateTo(editor);
    }

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
