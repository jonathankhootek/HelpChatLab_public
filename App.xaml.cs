using Microsoft.UI.Xaml;
using HelpChat.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.IO;

namespace HelpChat;

public partial class App : Application
{
    private Window? _window;
    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        InitializeComponent();

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider(); // Ensure Microsoft.Extensions.DependencyInjection is referenced in your project
    }

    private void ConfigureServices(ServiceCollection services)
    {
        // Register services
        services.AddSingleton<ChatService>();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow(_serviceProvider);
        _window.Activate();
    }
}