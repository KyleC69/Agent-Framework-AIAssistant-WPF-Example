using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using AgentOrch.ChatApp.Wpf.Services.Agents;
using AgentOrch.ChatApp.Wpf.ToolFunctions;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;



namespace AgentOrch.ChatApp.Wpf.ViewModels;


public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly IAgentOrchestrator _agentOrchestrator;
    private readonly ILoggerFactory? _factory;
    private readonly IAsyncRelayCommand _sendCommand;
    private AIAgent? _agent;
    private string _draftMessage = string.Empty;
    private bool _isBusy;
    private bool _isConfigured;
    private CancellationTokenSource? _sendCts;
    private IAgentWorkflow? _workflow;








    public MainViewModel(ILoggerFactory factory)
    {
        _sendCommand = new AsyncRelayCommand(SendAsync, CanSend);
        SendCommand = _sendCommand;
        _factory = factory;
    }








    private AgentThread thread1 { get; set; }
    private AgentThread thread2 { get; set; }


    public ObservableCollection<ChatMessageViewModel> Messages { get; } = [];





    public string DraftMessage
    {
        get => _draftMessage;
        set
        {
            if (value == _draftMessage) return;

            _draftMessage = value;
            OnPropertyChanged();
            _sendCommand.NotifyCanExecuteChanged();
        }
    }





    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (value == _isBusy) return;

            _isBusy = value;
            OnPropertyChanged();
            _sendCommand.NotifyCanExecuteChanged();
        }
    }





    public ICommand SendCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;








    private static void Ui(Action action)
    {
        Dispatcher? dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.Invoke(action);
    }








    private bool CanSend()
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(DraftMessage) && _isConfigured;
    }








    public async Task<bool> ConfigureAgentAsync(CancellationToken cancellationToken = default)
    {
        var tools = new ToolBuilder().GetAiTools();

        IChatClient client =
            new OnnxChatClient("F:\\AI-Models\\phi3\\cpu_and_mobile\\cpu-int4-awq-block-128-acc-level-4");


        _agent = client.CreateAIAgent(
            "You are a senior programmer with 15 years of experience. You are polite and courteous and you try to solve the users tasks to the best of your ability.",
            "Phi-3-128k", "A multi turn agent for coding and tool operations", tools, _factory);

        thread1 = _agent.GetNewThread();
        thread2 = _agent.GetNewThread();


        _isConfigured = true;
        return true;
    }








    private async Task SendAsync()
    {
        var text = DraftMessage?.Trim();
        if (string.IsNullOrWhiteSpace(text)) return;



        IsBusy = true;
        _sendCts?.Cancel();
        _sendCts?.Dispose();
        _sendCts = new CancellationTokenSource();

        try
        {
            await _agent.RunAsync("Hello! I need help with a coding task.", thread2);
            await _agent.RunAsync("Can you assist me with using GitHub Models?", thread2);
        }
        finally
        {
            IsBusy = false;
        }
    }








    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}