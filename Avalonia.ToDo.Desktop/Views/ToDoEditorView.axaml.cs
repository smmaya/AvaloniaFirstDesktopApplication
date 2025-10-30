using Avalonia.Controls;
using Avalonia.ToDo.Desktop.ViewModels;

namespace Avalonia.ToDo.Desktop.Views;

public partial class ToDoEditorView : UserControl
{
    public ToDoEditorView()
    {
        InitializeComponent();
    }

    public ToDoEditorView(ToDoEditorViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}