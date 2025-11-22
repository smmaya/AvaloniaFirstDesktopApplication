namespace Avalonia.Shared.Interfaces;

public interface IAuthService
{
    Task<string?> LoginAsync(string username, string password);
    string? Token { get; }
    bool IsAuthenticated { get; }
}