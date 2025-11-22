using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ToDo.Desktop.Views;
using Avalonia.ToDo.Desktop.ViewModels;
using Avalonia.Shared.Interfaces;
using Avalonia.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Avalonia.ToDo.Desktop.Helpers;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Avalonia.ToDo.Desktop;

public class App : Application
{
    private IHost AppHost { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddHttpClient("Gateway", c =>
                {
                    c.BaseAddress = new Uri("http://localhost:7000");
                });

                services.AddSingleton<IAuthService>(sp =>
                {
                    var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    return new AuthService(clientFactory.CreateClient("Gateway"));
                });
                
                services.AddTransient<AuthHeaderHandler>();

                services.AddHttpClient<IToDoService, ToDoService>(c =>
                    {
                        c.BaseAddress = new Uri("http://localhost:7000");
                    })
                    .AddHttpMessageHandler<AuthHeaderHandler>();

                services.AddSingleton<MainWindowViewModel>();
            })
            .Build();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var authService = AppHost.Services.GetRequiredService<IAuthService>();

            var loginViewModel = new LoginViewModel(authService, () =>
            {
                var mainViewModel = AppHost.Services.GetRequiredService<MainWindowViewModel>();
                desktop.MainWindow = new MainWindow(mainViewModel);
                desktop.MainWindow.Show();

                foreach (var window in desktop.Windows)
                {
                    if (window is LoginWindowView loginWindow)
                    {
                        loginWindow.Close();
                        break;
                    }
                }

                return Task.CompletedTask;
            });

            desktop.MainWindow = new LoginWindowView(loginViewModel);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
