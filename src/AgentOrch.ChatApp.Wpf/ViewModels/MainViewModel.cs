using System;
using System.Threading;
using System.Threading.Tasks;

using AgentOrchestration.Wpf.Orchestration;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.InProc;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using ChatHistory = AgentOrchestration.Wpf.Models.ChatHistory;
using ChatRole = Microsoft.Extensions.AI.ChatRole;




namespace AgentOrchestration.Wpf.ViewModels;





public sealed class MainViewModel : BaseViewModel
{
    private static ILoggerFactory? _factory;

    private readonly IRelayCommand _cancelCommand;
    private readonly IWorkflowExecutionEnvironment _environment;
    private readonly InProcessExecutionEnvironment _inproc;

    private readonly ILogger<MainViewModel> _logger;
    private readonly CheckpointManager _manager;
    private readonly IAsyncRelayCommand _sendCommand;

    private bool _isConfigured;
    private CancellationTokenSource _sendCts = new();








    public MainViewModel(ILoggerFactory factory)
    {
        Messages = [];
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
        _sendCommand = new AsyncRelayCommand(SendAsync, CanSend);
        SendCommand = _sendCommand;
        _cancelCommand = new RelayCommand(CancelSend, CanCancel);
        CancelCommand = _cancelCommand;
        _logger = factory.CreateLogger<MainViewModel>();


        _manager = CheckpointManager.CreateInMemory();
        _inproc = InProcessExecution.Lockstep;
        _environment = _inproc.WithCheckpointing(_manager);
    }








    public ChatHistory Messages { get; set; }

    public bool IsModelReady { get; set; }








    public async Task<bool> ConfigureAgentAsync(CancellationToken cancellationToken = default)
    {


        OnPropertyChanged();
        _sendCommand.NotifyCanExecuteChanged();
        _cancelCommand.NotifyCanExecuteChanged();
        _isConfigured = true;
        return true;
    }








    #region Message handling and UI helpers

    private void AddDecoratedMessage(ChatMessage message)
    {
        string? sender = TryGetAgentName(message);
        string prefix = BuildSenderPrefix(sender, message.Role);
        string content = GetMessageText(message);

        if (!string.IsNullOrEmpty(prefix) && !content.StartsWith(prefix, StringComparison.Ordinal))
        {
            Messages.Add(new ChatMessage(message.Role, prefix + content));
            return;
        }

        Messages.Add(message);
    }








    private static string BuildSenderPrefix(string? sender, ChatRole role)
    {
        return !string.IsNullOrWhiteSpace(sender) ? $"{sender} ({role}): " : $"{role}: ";
    }








    private static bool MessageHasPrefix(ChatMessage message, string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
        {
            return true;
        }

        string content = GetMessageText(message);
        return content.StartsWith(prefix, StringComparison.Ordinal);
    }








    private static string GetMessageText(ChatMessage message)
    {
        return message.Contents is null || message.Contents.Count == 0 ? string.Empty : string.Concat(message.Contents);
    }








    private static string? TryGetAgentName(object? source)
    {
        if (source is null)
        {
            return null;
        }

        object? agentName = source.GetType().GetProperty("AgentName")?.GetValue(source);
        if (agentName is string name && !string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        object? agent = source.GetType().GetProperty("Agent")?.GetValue(source);
        if (agent is not null)
        {
            object? nameValue = agent.GetType().GetProperty("Name")?.GetValue(agent);
            if (nameValue is string agentNameValue && !string.IsNullOrWhiteSpace(agentNameValue))
            {
                return agentNameValue;
            }
        }

        object? authorName = source.GetType().GetProperty("AuthorName")?.GetValue(source);
        if (authorName is string author && !string.IsNullOrWhiteSpace(author))
        {
            return author;
        }

        object? fallbackName = source.GetType().GetProperty("Name")?.GetValue(source);
        return fallbackName is string fallback && !string.IsNullOrWhiteSpace(fallback) ? fallback : null;
    }








    /// <summary>
    ///     Sends the current draft message asynchronously using the configured AI agent.
    /// </summary>
    /// <remarks>
    ///     This method processes the draft message, interacts with the AI agent using multiple threads,
    ///     and handles the response. It ensures that the operation is performed only when the agent is
    ///     properly configured and the application is not busy.
    /// </remarks>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the AI agent is not configured before invoking this method.
    /// </exception>
    private async Task SendAsync()
    {

        IsBusy = true;
        _sendCts = new CancellationTokenSource();



        try
        {
            AIAgent flow = DoubleSequentialWorkflow.EntryPoint.CreateWorkflow().AsAIAgent();

            AgentResponse response = await flow.RunAsync(new ChatMessage(ChatRole.User, UserMessage));

            foreach (ChatMessage message in response.Messages)
            {
                Messages.AddAssistantMessage(message.Text);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Send operation canceled.");
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            _logger.LogError(ex, "Error while sending the message.");
            throw;
        }
        finally
        {
            IsBusy = false;

            // Cancel AFTER the workflow has fully completed
            _sendCts.Cancel();
            _sendCts.Dispose();
        }

        _logger.LogTrace("Exiting SendAsync");
    }

    #endregion





    #region UI specific properties and commands

    public IAsyncRelayCommand SendCommand { get; }

    public IRelayCommand CancelCommand { get; }





    public string UserMessage
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
            _sendCommand.NotifyCanExecuteChanged();
        }
    } = string.Empty;





    public bool IsBusy
    {
        get;
        private set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
            _sendCommand.NotifyCanExecuteChanged();
            _cancelCommand.NotifyCanExecuteChanged();
        }
    }








    private bool CanSend()
    {
        return !IsBusy
               && _isConfigured
               && !string.IsNullOrWhiteSpace(UserMessage);
    }








    private bool CanCancel()
    {
        return IsBusy;
    }








    private void CancelSend()
    {
        if (!IsBusy)
        {
            return;
        }

        _sendCts.Cancel();
    }

    #endregion



}