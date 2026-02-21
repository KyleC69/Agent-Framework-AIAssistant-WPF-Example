using System.Windows;

using AgentOrch.ChatApp.Wpf.Agents;
using AgentOrch.ChatApp.Wpf.Services;
using AgentOrch.ChatApp.Wpf.Services.Agents;
using AgentOrch.ChatApp.Wpf.ViewModels;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OllamaSharp;




namespace AgentOrch.ChatApp.Wpf;





/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{

    private IHost _host = null!;





    public static IServiceProvider Services
    {
        get { return CurrentApp._host.Services; }
    }





    public static App CurrentApp
    {
        get { return (App)Current!; }
    }








    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        if (IsConsoleEnabled())
        {
            ConsoleWindow.Ensure();
        }

        //Set environment to development for local testing
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        var environmentName = GetEnvironmentName();

        IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environmentName}.json", false, true)
                .AddEnvironmentVariables()
                .Build();

        _host = Host.CreateDefaultBuilder()
                .UseEnvironment(environmentName)
                .ConfigureAppConfiguration(builder =>
                {
                    builder.Sources.Clear();
                    builder.AddConfiguration(configuration);
                })
                .ConfigureServices((_, services) => ConfigureServices(services, configuration))
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
    {
        return Services.GetService(typeof(T)) as T;
    }








    public static T GetRequiredService<T>() where T : class
    {
        return Services.GetRequiredService<T>();
    }





    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
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

        services.AddSingleton<IChatClient>(_ => new OllamaApiClient("http://localhost:11434", "llama3.2:1b"));
        services.AddSingleton<ToolAgent1>();
        services.AddSingleton<ToolAgent2>();
        services.AddSingleton<ToolAgent3>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<PlanningAgent>();
        services.AddSingleton<SeniorCoderAgent>();
        services.AddSingleton<SeniorCoderAgent2>();
        services.AddSingleton<QualityControlAgent>();
        services.AddSingleton<OrchestratorAgent>(sp =>
        {
            var t1 = sp.GetRequiredService<ToolAgent1>();
            var t2 = sp.GetRequiredService<ToolAgent2>();
            var t3 = sp.GetRequiredService<ToolAgent3>();
            var agentsAsTools = new[] { t3.Agent, t2.Agent, t1.Agent };

            return new OrchestratorAgent(
                sp.GetRequiredService<IChatClient>(),
                agentsAsTools,
                sp.GetRequiredService<ILoggerFactory>());
        });
        services.AddSingleton<CooperativeAgentsInitializer>();
    }








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
        if (Enum.TryParse(value, true, out LogLevel level))
        {
            return level;
        }

        // Default: keep console less noisy unless explicitly requested.
        return LogLevel.Trace;
    }
}