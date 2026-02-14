using System.Diagnostics;

using AgentOrch.ChatApp.Wpf.ToolFunctions;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;



namespace AgentOrch.ChatApp.Wpf.Models;


public enum AgentType
{
    EmailAnalysis,
    EmailAssistant,
    EmailSummary,
    Programmer,
    TeamLeader,
    ProjectManager,
    ProjectLead,

    //Special agent used to test sample tools examples
    ToolTester,
    Developer1,
    Developer2,
    Designer,
    Tester,
    Recruiter,
    HumanResources,
    Sales,
    Marketing,
    CustomerSupport,
    Operations,
    Finance,
    Legal,
    Administrator,

    /// <summary>
    ///     Use this type for a general purpose AI assistant. Be sure to pass in instructions to refine the assistant's
    ///     behavior.
    /// </summary>
    GeneralAssistant,
    Custom,

    /// <summary>
    ///     Represents an AI agent type designed for planning and organizing tasks or workflows.
    /// </summary>
    /// <remarks>
    ///     The <see cref="AgentType.Planner" /> is typically used for scenarios requiring structured task management,
    ///     workflow orchestration, or strategic planning. It can be integrated into group chat workflows or used
    ///     independently to assist with planning-related activities.
    /// </remarks>
    Planner
}





/// <summary>
///     Provides methods and tools for creating and managing AI agents of various types.
/// </summary>
/// <remarks>
///     This class is responsible for initializing AI tools and creating instances of different types of AI agents.
///     It supports a variety of agent types, such as planners, developers, designers, testers, and more.
///     The agents can be configured with specific instructions and a chat client implementation.
/// </remarks>
public class Definitions
{

    //   private const string _path = "F:\\AI-Models\\phi3\\cpu_and_mobile\\cpu-int4-awq-block-128-acc-level-4";
    //   private const string _path2 = "F:\\AI-Models\\functiongemma-270m-it-ONNX\\onnx\\model_q4f16.onnx";

    // Using OnnxChatClient to load a local ONNX model for chat interactions
    //private readonly IChatClient _chatClient = new OnnxChatClient(_path);

    // Inject necessary services here, e.g., ILogger, etc.
    private readonly ILoggerFactory _factory = App.Services.GetRequiredService<ILoggerFactory>()!;
    private readonly IList<AITool> _tools;

    //Collection of AI tools available to agents
    private readonly AiFunctionTests tb = new();

    // Initialize tools here if they were not initialized in the constructor.








    public Definitions()
    {
        _tools = tb.GetSampleTools();
    }








    /// <summary>
    ///     Retrieves an <see cref="AIAgent" /> instance based on the specified parameters. This AIAgent factory defines
    ///     several common agent types with pre-configured settings.
    ///     Only the instructions and metadata differ between the various agent types. It is meant to demonstrate how to
    ///     quickly spin up different types of agents with different behaviors.
    ///     This factory supports a variety of agent types including Planner, Developer, Designer, Tester, Recruiter, Human
    ///     Resources, Team Leader, Project Manager, Programmer, and General Assistant.
    ///     You can easily add more types and extend the factory to change other aspects of the agent configuration as needed.
    ///     This factory also demonstrates how to customize the models internal settings like temperature, max tokens,
    ///     penalties, and tool usage, as long as the model allows it.
    /// </summary>
    /// <param name="instructions">
    ///     Optional instructions to customize the agent's behavior. If not provided, default instructions are used. You can
    ///     pass your own instructions here to override the defaults.
    /// </param>
    /// <param name="agentType">
    ///     The type of agent to retrieve. This determines the specific behavior and configuration of the returned agent.
    ///     This particular Agent factory supports the following AgentTypes: Planner, Developer1, Developer2, Designer, Tester,
    ///     Recruiter, HumanResources, TeamLeader, ProjectManager, Programmer, GeneralAssistant.
    ///     These types are not exhaustive nor are they constructed differently, only their initial instructions and metadata
    ///     differ.
    /// </param>
    /// <param name="chatClient">
    ///     An optional <see cref="IChatClient" /> instance to be used by the agent. If not provided, a default chat client is
    ///     used.
    ///     You can use any IChatClient implementation here. You may use a built-in client like AI21StudioChatClient,
    ///     AzureOpenAIChatClient, OpenAIChatClient, or a custom implementation like OnnxChatClient.
    ///     This example uses a hand-rolled OnnxChatClient configured to use a local model. You have the freedom to use almost
    ///     any built-in model connector as long as it implements IChatClient.
    /// </param>
    /// <returns>
    ///     An <see cref="AIAgent" /> instance configured for the specified <paramref name="agentType" />.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     Thrown when the specified <paramref name="agentType" /> is not supported.
    /// </exception>
    public AIAgent GetAgent(string? instructions, AgentType agentType, IChatClient? chatClient)
    {
        // Fallback to the default chat client if none is provided.
        //  chatClient ??=// _chatClient;

        Debug.Assert(chatClient != null, nameof(chatClient) + " != null");
        return agentType switch
        {
            AgentType.Planner => GetPlannerAgent(instructions, chatClient),
            AgentType.ToolTester => GetToolTesterAgent(instructions, chatClient),
            AgentType.Developer1 => GetDeveloperAgent1(instructions, chatClient),
            AgentType.Developer2 => GetDeveloperAgent2(instructions, chatClient),
            AgentType.Designer => GetDesignerAgent(instructions, chatClient),
            AgentType.Tester => GetTesterAgent(instructions, chatClient),
            AgentType.Recruiter => GetRecruiterAgent(instructions, chatClient),
            AgentType.HumanResources => GetHumanResourcesAgent(instructions, chatClient),
            AgentType.TeamLeader => GetTeamLeaderAgent(instructions, chatClient),
            AgentType.ProjectManager => GetProjectManagerAgent(instructions, chatClient),
            AgentType.Programmer => GetProgrammerAgent(instructions, chatClient),
            AgentType.GeneralAssistant => GetGeneralAssistant(instructions ?? "You are a general purpose AI assistant. Help the user with whatever they need in a polite and courteous manner.", chatClient),

            _ => throw new NotSupportedException($"The agent type '{agentType}' is not supported.")
        };
    }








    private AIAgent GetToolTesterAgent(string? instructions, IChatClient chatClient)
    {
        ChatOptions chat = new()
        {
            Instructions = instructions ?? """
                                           You are a function tool agent. You are to call tools when you are instructed to so and reply with the info returned from the tool.
                                           If you do not understand a tool you are told to call then say so.

                                           """,
            MaxOutputTokens = 2000,
            ModelId = "Phi-3.5-Mini",
            AllowMultipleToolCalls = true,
            ToolMode = ChatToolMode.Auto,
            Tools = _tools
        };

        ChatClientAgentOptions agentoptions = new()
        {
            Id = "ToolTester1",
            Name = "ToolTester",
            Description = "This agent is designed to test AI tools.",
            ChatOptions = chat
        };



        //     AIFunction toolAgent = new OnnxRuntimeGenAIChatClient(_path2).CreateAIAgent(agentoptions, _factory).AsAIFunction();

        //     OnnxRuntimeGenAIChatClient client = new(_path2);





        return chatClient.CreateAIAgent(agentoptions, _factory).AsBuilder().UseLogging(_factory).Build();
    }








    private AIAgent GetGeneralAssistant(string? v, IChatClient chatClient)
    {
        ChatOptions c = new()
        {
            ConversationId = "storm1",
            Instructions = v ?? "Your name if steve and you are the planner. Your job is to lay out the steps and tasks that are necessary to solve the users tasks.",
            Temperature = 1.1f,
            MaxOutputTokens = 5000,
            TopP = 1.0f,
            TopK = 1,
            FrequencyPenalty = 0.0f,
            PresencePenalty = 0.0f,
            Seed = 2756,
            ResponseFormat = null,
            ModelId = "Phi-3.5-Mini",
            StopSequences = null,
            AllowMultipleToolCalls = true,
            ToolMode = ChatToolMode.Auto,
            Tools = []
        };

        ChatClientAgentOptions ops = new()
        {
            Id = "Plan1",
            Name = "Steve",
            Description = "A Planner, configured to understand the users request and develop a detailed plan to achieve the users goals.",
            ChatOptions = c,
            UseProvidedChatClientAsIs = true
        };


        return chatClient.CreateAIAgent(ops, _factory).AsBuilder().UseLogging(_factory).Build();
    }








    private AIAgent GetProjectManagerAgent(string? instructions, IChatClient? chatClient)
    {
        ChatOptions chat = new()
        {
            ConversationId = "storm1",
            Instructions = instructions ?? "Your name is Samantha and you are a senior project manager in a large software company. Your job is to ensure that any code generated is accurate and solves the users goal in the best way, not the easiest.",
            Temperature = 1.0f,
            MaxOutputTokens = 5000,
            TopP = 1.0f,
            TopK = 1,
            FrequencyPenalty = 1.0f,
            PresencePenalty = 1.0f,
            Seed = 2756,
            ResponseFormat = ChatResponseFormat.Text,
            ModelId = "Phi-3.5-Mini",
            StopSequences = null, // What to use here?
            AllowMultipleToolCalls = true,
            ToolMode = ChatToolMode.Auto,
            Tools = []
        };

        ChatClientAgentOptions ops = new()
        {
            Id = "mgr1",
            Name = "Samantha",
            Description = "This Agent is configured to be a project manager in charge of software development",
            ChatOptions = chat,
            UseProvidedChatClientAsIs = true
        };


        return chatClient.CreateAIAgent(ops, _factory).AsBuilder().UseLogging(_factory).Build();
    }








    private AIAgent GetTeamLeaderAgent(string? instructions, IChatClient chatClient)
    {
        ChatOptions c = new()
        {
            ConversationId = "storm1",
            Instructions = instructions ?? string.Empty,
            Temperature = 1.0f,
            MaxOutputTokens = 5000,
            TopP = 1.0f,
            TopK = 1,
            FrequencyPenalty = 1.0f,
            PresencePenalty = 1.0f,
            Seed = 2756,
            ResponseFormat = ChatResponseFormat.Text,
            ModelId = "Phi-3.5-Mini",
            AllowMultipleToolCalls = true,
            ToolMode = ChatToolMode.Auto,
            Tools = []
        };


        ChatClientAgentOptions ops = new()
        {
            Id = "TeamLead1",
            Name = "TeamLeader",
            Description = "This agent coordinates and delegates work across the team.",
            ChatOptions = c,
            UseProvidedChatClientAsIs = false
        };


        return chatClient.CreateAIAgent(ops, _factory).AsBuilder().UseLogging(_factory).Build();
    }








    private static ChatClientAgent GetHumanResourcesAgent(string? instructions, IChatClient chatClient)
    {
        throw new NotImplementedException();
    }








    private ChatClientAgent GetSalesAgentstrubg(string? instructions, IChatClient chatClient)
    {
        throw new NotImplementedException();
    }








    private static ChatClientAgent GetRecruiterAgent(string? instructions, IChatClient chatClient)
    {
        throw new NotImplementedException();
    }








    private ChatClientAgent GetTesterAgent(string? instructions, IChatClient chatClient)
    {
        throw new NotImplementedException();
    }








    private AIAgent GetDesignerAgent(string? instructions, IChatClient chatClient)
    {
        throw new NotImplementedException();
    }








    private AIAgent GetDeveloperAgent2(string? instructions, IChatClient? chatClient)
    {
        ChatOptions c = new()
        {
            ConversationId = "storm1",
            Instructions = instructions ?? "Your name is William and you are a senior software developer with 20 years in the industry, you like your software well structured, deterministic and you are very thorough.",
            Temperature = 1.0f,
            MaxOutputTokens = 5000,
            TopP = 1.0f,
            TopK = 1,
            FrequencyPenalty = 1.0f,
            PresencePenalty = 1.0f,
            Seed = 2756,
            ResponseFormat = ChatResponseFormat.Text,
            ModelId = "Phi-3.5-Mini",
            AllowMultipleToolCalls = true,
            ToolMode = ChatToolMode.Auto,
            Tools = []
        };


        ChatClientAgentOptions ops = new()
        {
            Id = "Dev2",
            Name = "William",
            Description = "The is configured to be part of a team and works with other agents.",
            ChatOptions = c,
            UseProvidedChatClientAsIs = true
        };


        return chatClient.CreateAIAgent(ops, _factory).AsBuilder().UseLogging(_factory).Build();
    }








    private AIAgent GetDeveloperAgent1(string? instructions, IChatClient? chatClient)
    {
        ChatOptions c = new()
        {
            ConversationId = "storm1",
            Instructions = instructions ?? "You are a software developer you will help generate code as necessary.",
            Temperature = 1.0f,
            MaxOutputTokens = 5000,
            TopP = 1.0f,
            TopK = 1,
            FrequencyPenalty = 1.0f,
            PresencePenalty = 1.0f,
            Seed = 2756,
            ResponseFormat = ChatResponseFormat.Text,
            ModelId = "Phi-3.5-Mini",
            AllowMultipleToolCalls = true,
            ToolMode = ChatToolMode.Auto,
            Tools = []
        };


        ChatClientAgentOptions ops = new()
        {
            Id = "Dev1",
            Name = "Johnathon",
            Description = "A senior programmer configured to work in a team brain storming session",
            ChatOptions = c,
            UseProvidedChatClientAsIs = true
        };


        return chatClient.CreateAIAgent(ops, _factory).AsBuilder().UseLogging(_factory).Build();
    }








    private AIAgent GetPlannerAgent(string? instructions, IChatClient chatClient)
    {
        ChatOptions c = new()
        {
            ConversationId = "storm1",
            Instructions = instructions ?? "Your name if steve and you are the planner. Your job is to lay out the steps and tasks that are necessary to solve the users tasks.",
            Temperature = 1.1f,
            MaxOutputTokens = 5000,
            TopP = 1.0f,
            TopK = 1,
            FrequencyPenalty = 0.0f,
            PresencePenalty = 0.0f,
            Seed = 2756,
            ResponseFormat = null,
            ModelId = "Phi-3.5-Mini",
            StopSequences = null,
            AllowMultipleToolCalls = true,
            ToolMode = ChatToolMode.Auto,
            Tools = []
        };

        ChatClientAgentOptions ops = new()
        {
            Id = "Plan1",
            Name = "Steve",
            Description = "A Planner, configured to understand the users request and develop a detailed plan to achieve the users goals.",
            ChatOptions = c,
            UseProvidedChatClientAsIs = true
        };


        return chatClient.CreateAIAgent(ops, _factory).AsBuilder().UseLogging(_factory).Build();
    }








    /// <summary>
    ///     Create an enhanced email analysis agent.
    /// </summary>
    /// <returns>A ChatClientAgent configured for comprehensive email analysis</returns>
    private AIAgent GetProgrammerAgent(string? instructions, IChatClient? chatClient)
    {
        ChatOptions c = new()
        {
            ConversationId = "storm1",
            Instructions = instructions ?? string.Empty,
            Temperature = 0.8f,
            MaxOutputTokens = 5000,
            TopP = 0.8f,
            TopK = 1,
            FrequencyPenalty = 0.0f,
            PresencePenalty = 0.0f,
            Seed = 2756,
            ResponseFormat = ChatResponseFormat.Text,
            ModelId = "Phi-3.5-Mini",
            StopSequences = null, // What to use here?
            AllowMultipleToolCalls = true,
            ToolMode = ChatToolMode.Auto,
            Tools = []
        };

        return chatClient.CreateAIAgent(new ChatClientAgentOptions
        {
            Id = "Prog1",
            Name = "Alex",
            Description = "A senior programmer with 15 years of experience, polite and courteous, who tries to solve the users tasks to the best of their ability.",
            ChatOptions = c,
            UseProvidedChatClientAsIs = true
        }, _factory).AsBuilder().UseLogging(_factory).Build();
    }
}