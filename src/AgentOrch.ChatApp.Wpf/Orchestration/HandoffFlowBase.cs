using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using AgentOrchestration.Wpf.Models;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace AgentOrchestration.Wpf.Orchestration;





/// <notes>
///     IEnumerable
///     <ChatMessage>
///         usage has been replaced with <see cref="ChatHistory" />. ChatHistory provides a more
///         efficient and flexible way to manage chat messages. Several convenient methods have been added to ChatHistory
///         for easy management.
/// </notes>
internal class CodingAgent
{
    private readonly ChatClientAgent _agent;
    private readonly IChatClient _chatClient;
    private readonly ILoggerFactory _factory;








    /// <notes>
    ///     IEnumerable
    ///     <ChatMessage>
    ///         usage has been replaced with <see cref="ChatHistory" />. ChatHistory provides a more
    ///         efficient and flexible way to manage chat messages. Several convenient methods have been added to ChatHistory
    ///         for easy management.
    /// </notes>
    /// <param name="id">The unique identifier for the agent instance. If null, a default identifier is used.</param>
    /// <param name="name">The display name of the agent. If null, the base agent name is used.</param>
    /// <param name="prefix">An optional prefix to prepend to echoed messages. If null, no prefix is applied.</param>
    public CodingAgent(IChatClient chatClient, ILoggerFactory factory, string? id = null, string? name = null)
    {
        _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
        IdCore = id ?? Guid.NewGuid().ToString("N");
        Name = name ?? "CodingAgent";
        _agent = chatClient.AsAIAgent(loggerFactory: factory);
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }








    protected string? IdCore { get; set; }

    public string? Name { get; set; }

    public InMemoryChatHistoryProvider ChatHistoryProvider { get; } = new();








    protected async ValueTask<AgentSession> DeserializeSessionCoreAsync(JsonElement serializedState, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
    {
        return serializedState.Deserialize<CodingAgentSession>(jsonSerializerOptions) ?? await _agent.CreateSessionAsync(cancellationToken);
    }








    private ChatMessage UpdateSession(ChatMessage message, AgentSession? session = null)
    {
        ChatHistoryProvider.GetMessages(session).Add(message);

        return message;
    }








    protected virtual ChatHistory GetMessages(AgentRunOptions? options = null)
    {
        return [];
    }








    private sealed class CodingAgentSession : AgentSession
    {
        internal CodingAgentSession()
        {
        }








        [JsonConstructor]
        internal CodingAgentSession(AgentSessionStateBag stateBag) : base(stateBag)
        {
        }
    }
}