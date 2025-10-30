using Avalonia.Controls;
using Avalonia.ToDo.Desktop.ViewModels;

namespace Avalonia.ToDo.Desktop.Views;

public partial class ToDoDetailsView : UserControl
{
    public ToDoDetailsView()
    {
        InitializeComponent();
    }

    public ToDoDetailsView(ToDoDetailsViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}