using Avalonia.Shared.ModelDtos;

namespace Avalonia.ToDo.Desktop.Models
{
    public class ToDoDesktopDto : ToDoDto
    {
        public bool IsPlaceholder { get; set; }
        
        public bool IsNotPlaceholder => !IsPlaceholder;
    }
}