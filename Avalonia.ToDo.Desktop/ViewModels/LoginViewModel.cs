using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Shared.Interfaces;
using Avalonia.ToDo.Desktop.Helpers;

namespace Avalonia.ToDo.Desktop.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private readonly IAuthService _authService;
    private readonly Func<Task> _onLoginSuccessAsync;

    private string _username = "demo";
    private string _password = "demo";
    private string _errorMessage = string.Empty;
    private bool _isLoading;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Username)));
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Password)));
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading)));
        }
    }

    public ICommand LoginCommand { get; }

    public LoginViewModel(IAuthService authService, Func<Task> onLoginSuccessAsync)
    {
        _authService = authService;
        _onLoginSuccessAsync = onLoginSuccessAsync;

        LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => !IsLoading);
    }

    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;
        IsLoading = true;

        try
        {
            var token = await _authService.LoginAsync(Username, Password);
            
            if (!string.IsNullOrEmpty(token))
            {
                await _onLoginSuccessAsync();
            }
            else
            {
                ErrorMessage = "Invalid username or password";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
