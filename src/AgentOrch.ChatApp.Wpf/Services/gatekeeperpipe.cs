using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Agents.AI;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

using OllamaSharp;




// ------------------------------------------------------------
// DTOs
// ------------------------------------------------------------





public sealed class RetrievedChunk
{
    public Guid ChunkId { get; init; }
    public string Content { get; init; } = string.Empty;
    public float Score { get; init; }
}





public sealed class RuleFilterResult
{
    public bool IsAllowed { get; init; }
    public IReadOnlyList<string> Violations { get; init; } = Array.Empty<string>();
}





// ------------------------------------------------------------
// SQL Server 2025 Vector Retriever
// ------------------------------------------------------------





public sealed class SqlVectorRetriever
{
    private readonly string _connectionString;








    public SqlVectorRetriever(string connectionString)
    {
        _connectionString = connectionString;
    }








    public async Task<IReadOnlyList<RetrievedChunk>> RetrieveAsync(float[] queryEmbedding, int topK = 8)
    {
        List<RetrievedChunk> results = [];
        var embeddingLiteral = string.Join(",", queryEmbedding);

        var sql = $@"
            SELECT TOP (@topK)
                chunk_id,
                content,
                VECTOR_DISTANCE(embedding, VECTOR[{embeddingLiteral}], 'cosine') AS score
            FROM dbo.rag_policy_chunk
            ORDER BY embedding <-> VECTOR[{embeddingLiteral}];";

        using SqlConnection conn = new(_connectionString);
        using SqlCommand cmd = new(sql, conn);
        _ = cmd.Parameters.AddWithValue("@topK", topK);

        await conn.OpenAsync();
        using SqlDataReader? reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            results.Add(new RetrievedChunk
            {
                    ChunkId = reader.GetGuid(0),
                    Content = reader.GetString(1),
                    Score = (float)reader.GetDouble(2)
            });

        return results;
    }
}





public static class AgentPipelineDI
{
    public static Task RegisterDI(IServiceCollection services)
    {
        // ServiceCollection services = new();

        // SQL retriever
        _ = services.AddSingleton(new SqlVectorRetriever(
                "Server=Desktop-NC01091;Database=AIDataRAG;Trusted_Connection=True;Encrypt=False;"));

        // Ollama endpoints
        Uri endpoint = new(Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT") ?? "http://localhost:11434");
        var toolModel = Environment.GetEnvironmentVariable("OLLAMA_TOOL_MODEL") ?? "functiongemma";
        var mainModel = Environment.GetEnvironmentVariable("OLLAMA_MAIN_MODEL") ?? "llama3.1:8b";
        var filterModel = Environment.GetEnvironmentVariable("OLLAMA_FILTER_MODEL") ?? "qwen2.5-coder:1.5b-base";



        // Register models
        //   services.AddKeyedSingleton(new OllamaApiClient(endpoint, filterModel)); // classifier/filter/embedder
        //   services.AddSingleton(new OllamaApiClient(endpoint, mainModel)); // reranker/main
        //   services.AddSingleton(new OllamaApiClient(endpoint, toolModel)); // tool model

        ServiceProvider provider = services.BuildServiceProvider();

        // Build the agent with middleware
        IChatClient ollamaClient = new OllamaApiClient(endpoint, mainModel);








        // Adding middleware to the chat client level and building an agent on top of it
        ChatClientAgent originalAgent = ollamaClient
                .AsBuilder()
                .Use(ChatClientMiddleware, null)
                .BuildAIAgent(
                        "You are an AI assistant that helps people find information.",
                        tools: [AIFunctionFactory.Create(GetDateTime, nameof(GetDateTime))]);







        return Task.CompletedTask;
    }








    // ------------------------------------------------------------
    // Middleware Implementations
    // ------------------------------------------------------------








    // Function invocation middleware that logs before and after function calls.
    private static async ValueTask<object?> FunctionCallMiddleware(AIAgent agent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
    {
        Debug.WriteLine($"Function Name: {context!.Function.Name} - Middleware 1 Pre-Invoke");
        var result = await next(context, cancellationToken);
        Debug.WriteLine($"Function Name: {context!.Function.Name} - Middleware 1 Post-Invoke");

        return result;
    }








    // This middleware handles chat client lower level invocations. 
    // This is useful for handling agent messages before they are sent to the LLM and also handle any response messages from the LLM before they are sent back to the agent.
    // IMPORTANT: This middleware is at the chat client level, not the agent level. So it will be invoked for all agents using this chat client.
    // NOTE!!: This will be triggered for any agent using this chat client at every workflow step that involves chat client calls.
    private static async Task<ChatResponse> ChatClientMiddleware(IEnumerable<ChatMessage> message, ChatOptions? options, IChatClient innerChatClient, CancellationToken cancellationToken)
    {
        //Ideal place to add logging, telemetry, filtering, enforce guardrails or modify the message before sending to LLM
        Debug.WriteLine("Chat Client Middleware - Pre-Chat");

        // Call the inner chat client and send message with any modified data to LLM  and get the response from the LLM
        ChatResponse response = await innerChatClient.GetResponseAsync(message, options, cancellationToken);

        //Ideal place to verify response, apply formatting, or modify the response before sending back to the agent**
        Debug.WriteLine("Chat Client Middleware - Post-Chat");

        //Return the response back to the agent for further processing - This could be another middleware, another step in the workflow, or the final response to the end user.
        return response;
    }








    // There's no difference per-request middleware, except it's added to the agent and used for a single agent run.
    // This middleware logs function names before and after they are invoked.
    private static async ValueTask<object?> PerRequestFunctionCallingMiddleware(AIAgent agent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Agent Id: {agent.Id}");
        Console.WriteLine($"Function Name: {context!.Function.Name} - Per-Request Pre-Invoke");
        var result = await next(context, cancellationToken);
        Console.WriteLine($"Function Name: {context!.Function.Name} - Per-Request Post-Invoke");
        return result;
    }








    [Description("Get the weather for a given location.")]
    private static string GetWeather([Description("The location to get the weather for.")] string location)
    {
        return $"The weather in {location} is cloudy with a high of 15°C.";
    }








    [Description("The current datetime offset.")]
    private static string GetDateTime()
    {
        return DateTimeOffset.Now.ToString();
    }
}