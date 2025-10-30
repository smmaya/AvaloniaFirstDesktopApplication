using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.ToDo.Desktop.Views;

namespace Avalonia.ToDo.Desktop.Helpers;

public class RelayCommand : ICommand
{
    private readonly Func<object?, Task>? _executeAsync;
    private readonly Action<object?>? _execute;
    private readonly Predicate<object?>? _canExecute;

    public RelayCommand(Func<object?, Task> executeAsync, Predicate<object?>? canExecute = null)
    {
        _executeAsync = executeAsync;
        _canExecute = canExecute;
    }

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public event EventHandler? CanExecuteChanged;

    public async void Execute(object? parameter)
    {
        try
        {
            // throw new Exception("This is a command exception message.");
            if (!CanExecute(parameter))
            {
                return;
            }

            if (_executeAsync != null)
            {
                await _executeAsync(parameter);
            }
            else
            {
                _execute?.Invoke(parameter);
            }
        }
        catch (Exception e)
        {
            if (Application.Current?.ApplicationLifetime is Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = desktop.MainWindow as MainWindow;
                mainWindow?.ShowAlert("An error occurred:\n" + e.Message);
            }
        }
    }
    
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}