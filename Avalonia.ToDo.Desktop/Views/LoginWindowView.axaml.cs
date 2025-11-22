using Avalonia.Controls;
using Avalonia.ToDo.Desktop.ViewModels;

namespace Avalonia.ToDo.Desktop.Views;

public partial class LoginWindowView : Window
{
    public LoginWindowView()
    {
        InitializeComponent();
    }

    public LoginWindowView(LoginViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}