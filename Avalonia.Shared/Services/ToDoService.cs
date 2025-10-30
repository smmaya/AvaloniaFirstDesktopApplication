using System.Net.Http.Json;
using Avalonia.Shared.Interfaces;
using Avalonia.Shared.ModelDtos;

namespace Avalonia.Shared.Services;

public class ToDoService : IToDoService
{
    private readonly HttpClient _httpClient;

    public ToDoService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<IEnumerable<ToDoDto>> GetAllAsync()
        => await _httpClient.GetFromJsonAsync<IEnumerable<ToDoDto>>("api/todo") ?? Enumerable.Empty<ToDoDto>();

    public async Task<ToDoDto?> GetByIdAsync(int id)
        => await _httpClient.GetFromJsonAsync<ToDoDto>($"api/todo/{id}");

    public async Task<ToDoDto> CreateAsync(ToDoDto item)
    {
        var response = await _httpClient.PostAsJsonAsync("api/todo", item);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ToDoDto>() ?? item;
    }

    public async Task<ToDoDto> UpdateAsync(ToDoDto item)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/todo/{item.Id}", item);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ToDoDto>() ?? item;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/todo/{id}");
        return response.IsSuccessStatusCode;
    }
}
