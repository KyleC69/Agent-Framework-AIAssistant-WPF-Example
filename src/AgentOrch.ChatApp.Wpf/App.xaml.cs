using System;
using System.Windows;

using AgentOrchestration.Wpf.Agents;
using AgentOrchestration.Wpf.Models;
using AgentOrchestration.Wpf.Services;
using AgentOrchestration.Wpf.ToolFunctions;
using AgentOrchestration.Wpf.ViewModels;

using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.InProc;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OllamaSharp;




namespace AgentOrchestration.Wpf;





/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{

    private IHost _host = null!;





    public static IServiceProvider Services => CurrentApp._host.Services;





    public static App CurrentApp => (App)Current!;








    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        if (IsConsoleEnabled())
        {
            ConsoleWindow.Ensure();
        }

        //Set environment to development for local testing
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        string environmentName = GetEnvironmentName();

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
                    _ = builder.AddConfiguration(configuration);
                })
                .ConfigureServices((context, services) => ConfigureServices(services, configuration))
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
        _ = services.AddLogging(logging =>
        {

            _ = logging.AddConsole();
            _ = logging.AddDebug();
            _ = logging.AddJsonConsole();
            _ = logging.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
            });
            _ = logging.SetMinimumLevel(LogLevel.Trace);
        });

        OllamaOptions ollamaOptions = LoadOllamaOptions(configuration, "General");
        _ = services.AddSingleton<IChatClient>(_ => new OllamaApiClient(ollamaOptions.Url!, "llama3.2:1b"));
        CheckpointManager manager = CheckpointManager.CreateInMemory();
        InProcessExecutionEnvironment environment = InProcessExecution
                .Lockstep
                .WithCheckpointing(manager);

        _ = services.AddSingleton<IWorkflowExecutionEnvironment>(environment);
        _ = services.AddHttpClient();
        _ = services.AddSingleton<WebSearchPlugin>();
        _ = services.AddSingleton<MainWindow>();
        _ = services.AddSingleton<MainViewModel>();
        _ = services.AddSingleton<SeniorCoderAgent>();
        _ = services.AddSingleton<SeniorCoderAgent2>();
        _ = services.AddSingleton<OrchestratorAgent>();
        _ = services.AddSingleton<QualityControlAgent>();


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
        string? value = configuration["AGENTORCH_LOG_LEVEL"]
                    ?? Environment.GetEnvironmentVariable("AGENTORCH_LOG_LEVEL");
        if (Enum.TryParse(value, true, out LogLevel level))
        {
            return level;
        }

        // Default: keep console less noisy unless explicitly requested.
        return LogLevel.Trace;
    }








    private static OllamaOptions LoadOllamaOptions(IConfiguration configuration, string key)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        OllamaOptions? options = configuration.GetSection($"Ollama:{key}").Get<OllamaOptions>();
        return options is null
               || string.IsNullOrWhiteSpace(options.Url)
               || string.IsNullOrWhiteSpace(options.ModelId)
                ? throw new InvalidOperationException($"Missing required configuration for 'Ollama:{key}'.")
                : options;
    }
}