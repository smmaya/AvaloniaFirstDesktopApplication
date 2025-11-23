using Avalonia.Controls;
using Avalonia.ToDo.Desktop.ViewModels;

namespace Avalonia.ToDo.Desktop.Views;

public partial class LogPanelView: UserControl
{
    public LogPanelView()
    {
        InitializeComponent();
        DataContext = new LogPanelViewModel();
    }
}