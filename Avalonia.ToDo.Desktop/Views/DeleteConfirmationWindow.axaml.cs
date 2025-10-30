using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Avalonia.ToDo.Desktop.Views;

public partial class DeleteConfirmationWindow : Window
{
    public DeleteConfirmationWindow()
    {
        InitializeComponent();
    }

    private void Yes_Click(object? sender, RoutedEventArgs e) => Close(true);
    private void No_Click(object? sender, RoutedEventArgs e) => Close(false);
}