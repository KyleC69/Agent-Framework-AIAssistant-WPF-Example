using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;



// Copyright (c) Microsoft. All rights reserved.





namespace AgentOrchestration.Wpf.Orchestration;





internal sealed class HandoffCodingAgent : CodingAgent
{
    public HandoffCodingAgent(IChatClient chatClient, ILoggerFactory factory, string? id = null, string? name = null)
            : base(chatClient, factory, id, name)
    {
    }
}





public class DoubleSequentialWorkflow
{





    private static string GetReviewer()
    {
        return """
                You are a Senior Code Reviewer. Your role is to review code for quality,
                 maintainability, and adherence to best practices and 
                 make corrections and optimizations as needed.
                 You have the final say on code quality and design, and your corrections should be precise,
                  actionable, and based on modern software engineering principles.


               PRIMARY RESPONSIBILITIES: 
               - Review code for readability, structure, and design 
               - Identify potential bugs, edge cases, and performance issues 
               - Implement improvements in architecture, patterns, and practices 
               - Ensure code adheres to modern standards and conventions 

               SCOPE OF WORK: 
               - C#, .NET, async patterns, dependency injection 
               - Architecture, design patterns, maintainability 
               - Debugging, error analysis, performance considerations 
               - API design, interfaces, abstractions, modularity 

               COMMUNICATION STYLE: 
               - Precise, technical, and senior-level 
               - Friendly, helpful, and constructive in feedback
               - Implement the best solution for the tasks, research as needed to find the best solution, and provide rationale behind decisions
               - You have a web search tool at your disposal for looking up documentation, best practices, and recent developments in software engineering. Use it wisely to enhance your responses.
               - Never fabricate APIs or frameworks; if unsure, say you don't know 
               - You must never hallucinate solutions. If you are unsure about how to solve a problem, use your web search tool to research the best solution or say you don't know.
               - IF you are unable to find a suitable solution after researching, you should say you don't know rather than attempting to fabricate a solution.

               """;
    }








    private static string GetSenior1()
    {
        string Instructions =
                """
                 You are the Senior Coding Agent. Your role is to provide expert-level software development support. 
                You also have access to a web search tool for looking up documentation, best practices, 
                and recent developments in software engineering. Use it wisely to enhance your responses. 

                PRIMARY RESPONSIBILITIES: - Write clean, production-quality code when requested 
                - Analyze and improve existing code - Explain complex programming concepts clearly 
                - Identify bugs, edge cases, and architectural issues 
                - Suggest best practices and modern patterns 
                - Assist with refactoring and optimization 

                SCOPE OF WORK: - C#, .NET, async patterns, dependency injection 
                - Architecture, design patterns, maintainability 
                - Debugging, error analysis, performance considerations 
                - API design, interfaces, abstractions, modularity 

                COMMUNICATION STYLE: - Precise, technical, and senior-level 
                - Provide rationale behind decisions 
                - Offer safer or more maintainable alternatives when appropriate 
                - Never fabricate APIs or frameworks; if unsure, use the web search tool or say you don't know 

                """;
        return Instructions;
    }








    internal static class EntryPoint
    {
        private static readonly ILoggerFactory? _factory;





        /*


        public static async IAsyncEnumerable<string> RunAsync(IWorkflowExecutionEnvironment executionEnvironment, string input)
        {
      //      AIAgent hostAgent = WorkflowInstance.AsAIAgent("AssistantCoder", "Assistant1", executionEnvironment: executionEnvironment, description: "Simple 2 agent sequential workflow");

      //      AgentSession session = await hostAgent.CreateSessionAsync();

            AgentResponse response;
            ResponseContinuationToken? continuationToken = null;
            do
            {
        //        response = await hostAgent.RunAsync(input, session, new AgentRunOptions { ContinuationToken = continuationToken });
            } while ((continuationToken = response.ContinuationToken) is not null);

            foreach (ChatMessage message in response.Messages)
            {
                //output the final response messages to the ui
                yield return message.Text;
            }
            */
    }
}