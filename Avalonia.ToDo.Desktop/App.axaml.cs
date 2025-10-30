using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ToDo.Desktop.Views;
using Avalonia.ToDo.Desktop.ViewModels;
using Avalonia.Shared.Interfaces;
using Avalonia.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

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
                services.AddHttpClient<IToDoService, ToDoService>(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5119");
                });
                
                services.AddSingleton<MainWindowViewModel>();
            })
            .Build();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow(AppHost.Services.GetRequiredService<MainWindowViewModel>());
    
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}