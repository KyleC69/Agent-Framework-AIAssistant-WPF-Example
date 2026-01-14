using System.Windows;

using AgentOrch.ChatApp.Wpf.Services;
using AgentOrch.ChatApp.Wpf.ViewModels;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;



namespace AgentOrch.ChatApp.Wpf;


/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider Services => CurrentApp._host.Services;

    public static App CurrentApp => (App)Current!;

    private IHost _host = null!;








    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (IsConsoleEnabled()) ConsoleWindow.Ensure();
        //Set environment to development for local testing
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");

        var environmentName = GetEnvironmentName();
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile($"appsettings.{environmentName}.json", false, true)
            .AddEnvironmentVariables()
            .Build();

        _host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .UseEnvironment(environmentName)
            .ConfigureAppConfiguration(builder =>
            {
                builder.Sources.Clear();
                builder.AddConfiguration(configuration);
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton<IConfiguration>(configuration);

                //    services.AddSemanticKernelInfrastructure(configuration);

                services.AddLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddSimpleConsole(options =>
                    {
                        options.SingleLine = true;
                        options.TimestampFormat = "HH:mm:ss.fff ";
                    });
                    logging.SetMinimumLevel(GetLogLevel(configuration));
                });

                // UI
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();

        _host.Start();

        // Optional: surface a startup message
        var logger = Services.GetRequiredService<ILogger<App>>();
        logger.LogInformation("App started. Environment={Environment}. ConsoleMonitoring={ConsoleEnabled}",
            environmentName, IsConsoleEnabled());

        MainWindow mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            _host?.Dispose();
        }
        finally
        {
            base.OnExit(e);
        }
    }








    public static T? GetService<T>() where T : class
        => Services.GetService(typeof(T)) as T;

    public static T GetRequiredService<T>() where T : class
        => Services.GetRequiredService<T>();








    private static string GetEnvironmentName()
    {
        return Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
               ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
               ?? "Production";
    }








    private static bool IsConsoleEnabled()
    {
#if DEBUG
        return true;
#else
		return string.Equals(Environment.GetEnvironmentVariable("AGENTORCH_ENABLE_CONSOLE"), "1", StringComparison.OrdinalIgnoreCase);
#endif
    }








    private static LogLevel GetLogLevel(IConfiguration configuration)
    {
        var value = configuration["AGENTORCH_LOG_LEVEL"]
                    ?? Environment.GetEnvironmentVariable("AGENTORCH_LOG_LEVEL");
        if (Enum.TryParse(value, true, out LogLevel level)) return level;

        // Default: keep console less noisy unless explicitly requested.
        return LogLevel.Trace;
    }
}