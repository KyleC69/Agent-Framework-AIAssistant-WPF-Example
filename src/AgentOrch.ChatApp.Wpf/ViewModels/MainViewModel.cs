using System;
using System.Threading;
using System.Threading.Tasks;

using AgentOrchestration.Wpf.Agents;
using AgentOrchestration.Wpf.Models;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;




namespace AgentOrchestration.Wpf.ViewModels;





public sealed class MainViewModel : BaseViewModel
{
    private static ILoggerFactory? _factory;
    private readonly IRelayCommand _cancelCommand;
    private readonly ILogger<MainViewModel> _logger;
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


    }








    //Used to store UI messages
    public ChatHistory Messages { get; set; }








    public async Task<bool> ConfigureAgentAsync(CancellationToken cancellationToken = default)
    {


        OnPropertyChanged();
        _sendCommand.NotifyCanExecuteChanged();
        _cancelCommand.NotifyCanExecuteChanged();
        _isConfigured = true;
        return true;
    }








    #region Message handling and UI helpers

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
        _logger.LogTrace("Entering SendAsync");
        IsBusy = true;
        _sendCts = new CancellationTokenSource();

        try
        {

            WorkflowAgentOrchestrator AI = new(_factory!.CreateLogger<WorkflowAgentOrchestrator>());
            await foreach (string response in AI.RunGroupWorkflowStreamingAsync(UserMessage, _sendCts.Token))
            {
                Messages.AddAssistantMessage(response);
            }
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