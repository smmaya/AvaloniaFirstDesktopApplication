using Avalonia.Shared.ModelDtos;

namespace Avalonia.Shared.Interfaces;

public interface IToDoService
{
    Task<IEnumerable<ToDoDto>> GetAllAsync();
    Task<ToDoDto?> GetByIdAsync(int id);
    Task<ToDoDto> CreateAsync(ToDoDto item);
    Task<ToDoDto> UpdateAsync(ToDoDto item);
    Task<bool> DeleteAsync(int id);
}