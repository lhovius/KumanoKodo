using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using KumanoKodo.ViewModels;

namespace KumanoKodo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    private IConfiguration? _configuration;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{GetEnvironment()}.json", optional: true, reloadOnChange: true);

        _configuration = builder.Build();

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        if (_configuration == null)
            throw new InvalidOperationException("Configuration not initialized");

        services.AddLogging(builder =>
        {
            builder.AddDebug();
            builder.AddConsole();
        });

        // Register services
        services.AddSingleton<IConfiguration>(_configuration);
        services.AddSingleton<IDataAccess, DataAccess>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<LessonsViewModel>();
        services.AddSingleton<ProgressViewModel>();
        services.AddSingleton<QuizViewModel>();
    }

    private string GetEnvironment()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return string.IsNullOrEmpty(environment) ? "Development" : environment;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        _serviceProvider?.Dispose();
    }
}

