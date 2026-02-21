using AgentOrchestration.Wpf.ToolFunctions;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace AgentOrchestration.Wpf.Agents;





public sealed class SeniorCoderAgent
{
    private readonly ChatClientAgent _agent;








    /// <summary>
    ///     Initializes a new instance of the <see cref="SeniorCoderAgent" /> class.
    /// </summary>
    /// <param name="chatClient">
    ///     The chat client used to create and manage the AI agent.
    /// </param>
    /// <param name="factory">
    ///     The logger factory used for creating loggers to support logging functionality.
    /// </param>
    /// <remarks>
    ///     This constructor sets up a senior coding agent specializing in advanced software development,
    ///     code analysis, and implementation guidance. It configures the agent with specific options,
    ///     including chat history and AI context providers, reasoning capabilities, and tool integrations.
    /// </remarks>
    public SeniorCoderAgent(IChatClient chatClient, ILoggerFactory factory)
    {



        WebSearchPlugin websearch = new();







        _agent = chatClient.AsAIAgent(
                name: "SeniorCoderAgent",
                description: "A specialized agent for advanced software development, code analysis, and implementation guidance.",
                loggerFactory: factory,
                tools: [AIFunctionFactory.Create(websearch.WebSearch)],
                instructions: """
                              You are the Senior Coding Agent. Your role is to provide expert-level software development support.
                              You also have access to a web search tool for looking up documentation, best practices,
                              and recent developments in software engineering. Use it wisely to enhance your responses.

                              PRIMARY RESPONSIBILITIES:
                              - Write clean, production-quality code when requested
                              - Analyze and improve existing code
                              - Explain complex programming concepts clearly
                              - Identify bugs, edge cases, and architectural issues
                              - Suggest best practices and modern patterns
                              - Assist with refactoring and optimization

                              SCOPE OF WORK:
                              - C#, .NET, async patterns, dependency injection
                              - Architecture, design patterns, maintainability
                              - Debugging, error analysis, performance considerations
                              - API design, interfaces, abstractions, modularity

                              COMMUNICATION STYLE:
                              - Precise, technical, and senior-level
                              - Provide rationale behind decisions
                              - Offer safer or more maintainable alternatives when appropriate
                              - Never fabricate APIs or frameworks; if unsure, use the web search tool or say you don't know

                              Your output should always be accurate, grounded, and production-grade.
                              """
        );


    }








    public AIAgent Agent
    {
        get { return _agent; }
    }





    /*


    private AIAgent CreateAgent(IChatClient chatClient, ILoggerFactory factory)
    {
        var instructions = """
                           You are the Senior Coding Agent. Your role is to provide expert-level software development support. You also have access to a web search tool for looking up documentation, best practices,
                           and recent developments in software engineering. Use it wisely to enhance your responses.


                           PRIMARY RESPONSIBILITIES:
                           - Write clean, production-quality code when requested
                           - Analyze and improve existing code
                           - Explain complex programming concepts clearly
                           - Identify bugs, edge cases, and architectural issues
                           - Suggest best practices and modern patterns
                           - Assist with refactoring and optimization

                           SCOPE OF WORK:
                           - C#, .NET, async patterns, dependency injection
                           - Architecture, design patterns, maintainability
                           - Debugging, error analysis, performance considerations
                           - API design, interfaces, abstractions, modularity

                           COMMUNICATION STYLE:
                           - Precise, technical, and senior-level
                           - Provide rationale behind decisions
                           - Offer safer or more maintainable alternatives when appropriate
                           - Never fabricate APIs or frameworks; if unsure, say so

                           Your output should always be accurate, grounded, and production-grade.
                           """;
        WebSearchPlugin websearch = new();


        ChatClientAgent agent = chatClient.AsAIAgent(new ChatClientAgentOptions
        {
                Name = "SeniorCoder", //TODO: Set by the user in the UI - ensure unique across agents
                Description = "A senior coding agent specializing in advanced software development, code analysis, and implementation guidance.",
                ChatOptions = new ChatOptions
                {
                        Instructions = instructions,
                        Temperature = 0.7f,
                        MaxOutputTokens = 1500,
                        ToolMode = ChatToolMode.Auto,
                        Tools = [AIFunctionFactory.Create(websearch.WebSearch)]
                }
        });




        return agent;
    }
    */
}