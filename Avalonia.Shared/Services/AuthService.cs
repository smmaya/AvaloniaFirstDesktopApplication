using System.Net.Http.Json;
using Avalonia.Shared.Interfaces;

namespace Avalonia.Shared.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    
    public string? Token { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/auth/token", new
            {
                username,
                password
            });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
                Token = result?.AccessToken;
                return Token;
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return null;
    }

    private record TokenResponse(string AccessToken, string TokenType, int ExpiresIn);
}