using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ToDo.Desktop.ViewModels;

namespace Avalonia.ToDo.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    
    public MainWindow(MainWindowViewModel viewModel) : this()
    {
        DataContext = viewModel;
        WindowState = WindowState.Maximized;
    }
    
    public async void ShowAlert(string message, int durationMs = 5000)
    {
        try
        {
            AlertMessage.Text = message;

            // Prepare before showing
            BottomAlert.Opacity = 0;
            BottomAlert.Margin = new Thickness(0, 0, 0, 10);
            BottomAlert.IsVisible = true;

            await Task.Delay(10); // allow layout to update

            // Animate in
            BottomAlert.Opacity = 1;
            BottomAlert.Margin = new Thickness(0, 0, 0, 30);

            await Task.Delay(durationMs);

            // Animate out
            BottomAlert.Opacity = 0;
            BottomAlert.Margin = new Thickness(0, 0, 0, 0);

            await Task.Delay(600); // same as animation duration

            BottomAlert.IsVisible = false;
        }
        catch
        {
            // ignored
        }
    }

    private async void CloseAlert_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            BottomAlert.Opacity = 0;
            BottomAlert.Margin = new Thickness(0, 0, 0, 0);
            await Task.Delay(600);
            BottomAlert.IsVisible = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}