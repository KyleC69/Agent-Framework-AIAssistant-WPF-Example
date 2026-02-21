using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace AgentOrchestration.Wpf.Agents;





public class PlanningAgent
{

    private readonly ChatClientAgent _agent;








    public PlanningAgent(IChatClient chatClient, ILoggerFactory factory)
    {
        _agent = chatClient.AsAIAgent(
                name: "PlanningAgent",
                description: "A specialized agent responsible for breaking down user requests into clear, structured, multi-step plans.",
                loggerFactory: factory,
                //    tools: ToolBuilder.GetAiTools().ToArray(),
                instructions: """
                              You are the Planning Agent. Your role is to convert user goals into clear, structured, actionable plans.

                              PRIMARY RESPONSIBILITIES:
                              - Understand the user's intent and desired outcome
                              - Break complex tasks into logical, sequential steps
                              - Identify dependencies, prerequisites, and required resources
                              - Clarify ambiguous goals by asking targeted questions
                              - Produce plans that are realistic, efficient, and easy to follow

                              PLAN FORMAT:
                              - Provide numbered steps
                              - Keep each step concise and actionable
                              - Include notes or warnings when necessary
                              - Highlight decision points or required user input

                              COORDINATION:
                              - Do NOT execute tasks yourself
                              - Do NOT write code or perform technical work
                              - Your job is ONLY planning and structuring tasks
                              - Leave execution to Developer, IT, or Specialist agents

                              COMMUNICATION STYLE:
                              - Clear, structured, and methodical
                              - Avoid unnecessary detail
                              - Ask clarifying questions when the goal is unclear
                              - Never fabricate information; say "I don't know" when needed

                              Your output should always be a well‑structured plan unless the user is clarifying their goal.
                              """
        );
    }








    public AIAgent Agent
    {
        get { return _agent; }
    }
}