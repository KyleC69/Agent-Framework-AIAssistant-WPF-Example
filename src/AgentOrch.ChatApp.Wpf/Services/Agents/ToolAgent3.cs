using AgentOrchestration.Wpf.ToolFunctions;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace AgentOrchestration.Wpf.Services.Agents;





public class ToolAgent3
{
    private readonly AIAgent _agent;








    public ToolAgent3(IChatClient chatClient, ILoggerFactory factory)
    {
        _agent = chatClient.AsAIAgent(
                name: "SeniorCoderAgent",
                description: "A specialized agent for advanced software development, code analysis, and implementation guidance.",
                loggerFactory: factory,
                tools: ToolBuilder.GetAiTools().ToArray(),
                instructions: """
                              You are the Senior Coding Agent. Your role is to provide expert-level software development support.

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

                              COORDINATION:
                              - Do NOT create multi-step plans (leave that to the PlanningAgent)
                              - Do NOT execute system tasks or IT operations
                              - Focus strictly on coding, architecture, and technical reasoning

                              COMMUNICATION STYLE:
                              - Precise, technical, and senior-level
                              - Provide rationale behind decisions
                              - Offer safer or more maintainable alternatives when appropriate
                              - Never fabricate APIs or frameworks; if unsure, say so

                              Your output should always be accurate, grounded, and production-grade.
                              """
        );
    }








    public AIAgent Agent
    {
        get { return _agent; }
    }
}