using System.Windows.Input;

using AgentOrch.ChatApp.Wpf.Services;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;

using ChatHistory = AgentOrch.ChatApp.Wpf.Models.ChatHistory;




namespace AgentOrch.ChatApp.Wpf.ViewModels;





public sealed class MainViewModel : BaseViewModel
{
    private readonly IRelayCommand _cancelCommand;

    private readonly ILoggerFactory _factory;
    private readonly ILogger<MainViewModel> _logger;
    private readonly IAsyncRelayCommand _sendCommand;
    private bool _isConfigured;
    private CancellationTokenSource _sendCts = new();
    private AgentSession _thread;








    public MainViewModel(ILoggerFactory factory)
    {
        Messages = [];
        ArgumentNullException.ThrowIfNull(factory);
        _sendCommand = new AsyncRelayCommand(SendAsync, CanSend);
        SendCommand = _sendCommand;
        _cancelCommand = new RelayCommand(CancelSend, CanCancel);
        CancelCommand = _cancelCommand;
        _factory = factory;
        _logger = factory.CreateLogger<MainViewModel>();
    }








    public ChatHistory Messages { get; set; }








    public async Task<bool> ConfigureAgentAsync(CancellationToken cancellationToken = default)
    {

        _isConfigured = true;
        this.OnPropertyChanged();
        _sendCommand.NotifyCanExecuteChanged();
        _cancelCommand.NotifyCanExecuteChanged();
        return true;
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
            AgentCoopDetailed coop = App.GetRequiredService<AgentCoopDetailed>();

            var results = await coop.BuildAgentCoopDetailedAsync(UserMessage);

            Messages.AddRange(results);

            //   var decision = gatekeeper.Classify("How do I reset my password");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Send operation canceled.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while sending the message.");
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid argument while sending the message.");
            throw;
        }
        finally
        {
            IsBusy = false;
            _sendCts.Cancel();
            _sendCts.Dispose();
        }

        _logger.LogTrace("Exiting Send method");
    }








    #region UI specific properties and commands

    public ICommand SendCommand { get; }

    public ICommand CancelCommand { get; }





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
            this.OnPropertyChanged();
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
            this.OnPropertyChanged();
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