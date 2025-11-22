using Avalonia.Shared.ModelDtos;

namespace Avalonia.Shared.Interfaces;

public interface IToDoService
{
    public Task<IEnumerable<ToDoDto>> GetAllAsync();
    public Task<ToDoDto?> GetByIdAsync(int id);
    public Task<ToDoDto> CreateAsync(ToDoDto item);
    public Task<ToDoDto> UpdateAsync(ToDoDto item);
    public Task<bool> DeleteAsync(int id);
}