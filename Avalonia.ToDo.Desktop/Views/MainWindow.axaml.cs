using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ToDo.Desktop.ViewModels;

namespace Avalonia.ToDo.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        viewModel.MainWindow = this;
        WindowState = WindowState.Maximized;
    }
    
    public async void ShowAlert(string message, int durationMs = 5000)
    {
        AlertMessage.Text = message;
        BottomAlert.IsVisible = true;
        
        await Task.Delay(durationMs);
        BottomAlert.IsVisible = false;
    }

    private void CloseAlert_Click(object? sender, RoutedEventArgs e)
    {
        BottomAlert.IsVisible = false;
    }
}