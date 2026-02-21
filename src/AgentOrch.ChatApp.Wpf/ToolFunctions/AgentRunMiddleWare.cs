using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

using ChatMessage = Microsoft.Extensions.AI.ChatMessage;




namespace AgentOrchestration.Wpf.ToolFunctions;





internal class AgentRunMiddleWare
{


    internal void Run()
    {
        /*
                AIAgent baseAgent = new OnnxChatClient(
                        new Uri("https://<myresource>.openai.azure.com"),
                        new ApiKeyCredential("NO_KEY"))
                    .GetChatClient("gpt-4o-mini")
                    .CreateAIAgent(
                        "You are an AI assistant that helps people find information.",
                        tools: [AIFunctionFactory.Create(GetDateTime, nameof(GetDateTime))]);


                AIAgent middlewareEnabledAgent = baseAgent
                    .AsBuilder()
                    .Use(CustomAgentRunMiddleware, null)
                    .Build();



                */
    }








    public void run2()
    {
    }








    private async Task<AgentResponse> CustomAgentRunMiddleware(
            IEnumerable<ChatMessage> messages,
            AgentSession? thread,
            AgentRunOptions? options,
            AIAgent innerAgent,
            CancellationToken cancellationToken)
    {
        Console.WriteLine($"Input: {messages.Count()}");
        AgentResponse response = await innerAgent.RunAsync(messages, thread, options, cancellationToken).ConfigureAwait(false);
        Console.WriteLine($"Output: {response.Messages.Count}");
        return response;
    }








    private async Task<ChatResponse> CustomChatClientMiddleware(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options,
            IChatClient innerChatClient,
            CancellationToken cancellationToken)
    {
        Console.WriteLine($"Input: {messages.Count()}");
        ChatResponse response = await innerChatClient.GetResponseAsync(messages, options, cancellationToken);
        Console.WriteLine($"Output: {response.Messages.Count}");

        return response;
    }
}