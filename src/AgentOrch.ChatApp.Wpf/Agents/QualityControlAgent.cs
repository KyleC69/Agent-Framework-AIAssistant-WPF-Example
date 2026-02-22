using AgentOrchestration.Wpf.ToolFunctions;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace AgentOrchestration.Wpf.Agents;





public class QualityControlAgent : BaseAgent
{


    private static readonly string instructions = """
                                                  You are the Quality Control Agent. Your role is to review code and validate the work produced by other agents.

                                                  PRIMARY RESPONSIBILITIES:
                                                  - Evaluate responses for correctness, clarity, and completeness
                                                  - Identify logical errors, inconsistencies, or missing details
                                                  - Suggest improvements to structure, readability, and accuracy
                                                  - Ensure outputs follow best practices and user requirements
                                                  - Flag unsafe, ambiguous, or low‑quality content

                                                  SCOPE OF WORK:
                                                  - Code review and technical validation
                                                  - Documentation and explanation quality checks
                                                  - Plan and workflow validation
                                                  - Consistency and correctness verification

                                                  COORDINATION:
                                                  - Do NOT create new solutions unless asked
                                                  - Do NOT execute tasks belonging to Developer or Planner agents
                                                  - Focus strictly on reviewing and improving existing outputs

                                                  COMMUNICATION STYLE:
                                                  - Direct, constructive, and detail‑oriented
                                                  - Provide clear reasoning for each correction
                                                  - Offer improved versions when appropriate
                                                  - Never fabricate information; say “I don’t know” when needed

                                                  """;








    public QualityControlAgent(IChatClient chatClient, ILoggerFactory factory)
    {
        _agent = chatClient.AsAIAgent(
                name: "QualityControlAgent",
                description: "A specialized agent responsible for reviewing and validating outputs for correctness, clarity, and quality.",
                loggerFactory: factory,
                tools: [AIFunctionFactory.Create(webSearch.WebSearch)],
                instructions: instructions
        );
    }








    private WebSearchPlugin webSearch => App.GetRequiredService<WebSearchPlugin>();
}