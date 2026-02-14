using System.Reflection;

using AgentOrch.ChatApp.Wpf.ToolFunctions;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;



namespace AgentOrch.ChatApp.Wpf.Services.Agents;


public sealed class WorkflowAgentOrchestrator(IChatClient chatClient, IServiceProvider services) : IAgentOrchestrator
{
    private readonly IChatClient _chatClient = chatClient;
    private readonly IServiceProvider _services = services;
}





public interface IAgentWorkflow
{
    IAsyncEnumerable<ChatMessage> RunAsync(ChatMessage input, CancellationToken cancellationToken = default);








    IAsyncEnumerable<ChatResponseUpdate> RunStreamingAsync(ChatMessage input,
        CancellationToken cancellationToken = default);
}





internal sealed class SingleChatWorkflow(
    IChatClient chatClient,
    AgentDefinition definition,
    IReadOnlyList<object> tools) : IAgentWorkflow
{
    private readonly IChatClient _chatClient = chatClient;
    private readonly AgentDefinition _definition = definition;
    private readonly IReadOnlyList<object> _tools = tools;








    public async IAsyncEnumerable<ChatMessage> RunAsync(ChatMessage input,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var instructions = _definition.Instructions;

        var messages = new List<ChatMessage>();
        if (!string.IsNullOrWhiteSpace(instructions)) messages.Add(new ChatMessage(ChatRole.System, instructions));

        messages.Add(input);

        // If the underlying chat client supports tools via options (common pattern in Microsoft.Extensions.AI),
        // attach them when possible. This is reflection-based to avoid binding to preview-only APIs.
        ChatResponse response = await ToolingSupport
            .GetResponseWithToolsAsync(_chatClient, messages, _tools, cancellationToken).ConfigureAwait(false);

        // API shape differs across preview versions; prefer enumerating response messages.
        PropertyInfo? messagesProp = response.GetType().GetProperty("Messages");
        if (messagesProp?.GetValue(response) is IEnumerable<ChatMessage> many)
        {
            foreach (ChatMessage m in many) yield return m;

            yield break;
        }

        PropertyInfo? messageProp = response.GetType().GetProperty("Message");
        if (messageProp?.GetValue(response) is ChatMessage single) yield return single;
    }








    public async IAsyncEnumerable<ChatResponseUpdate> RunStreamingAsync(
        ChatMessage input,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var instructions = _definition.Instructions;

        ChatOptions? options = ToolingSupport.CreateChatOptionsWithTools(_tools);

        if (!string.IsNullOrWhiteSpace(instructions))
            await foreach (ChatResponseUpdate update in ToolingSupport.GetStreamingResponseAsync(_chatClient,
                               new ChatMessage(ChatRole.System, instructions),
                               options,
                               cancellationToken))
                yield return update;

        await foreach (ChatResponseUpdate update in ToolingSupport.GetStreamingResponseAsync(_chatClient, input,
                           options, cancellationToken))
            yield return update;
    }
}





internal static class ToolRegistry
{
    public static IReadOnlyList<object> CreateTools(IServiceProvider services)
    {
        // Keep the existing tool classes (some are stubs) and let DI satisfy their constructors.
        // Add additional tools here as you implement more.
        return
        [
            ActivatorUtilities.CreateInstance<TimePlugin>(services),
            ActivatorUtilities.CreateInstance<AppInfoPlugin>(services),
            ActivatorUtilities.CreateInstance<FileSystemPlugin>(services),
            ActivatorUtilities.CreateInstance<WebSearchPlugin>(services),
            ActivatorUtilities.CreateInstance<SpecCheckPlugin>(services),
            ActivatorUtilities.CreateInstance<TaskDecomposerPlugin>(services),
            ActivatorUtilities.CreateInstance<CodeWriterPlugin>(services),
            ActivatorUtilities.CreateInstance<MathPlugin>(services)
        ];
    }
}





internal static class ToolingSupport
{
    public static IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IChatClient chatClient,
        ChatMessage input,
        ChatOptions? options,
        CancellationToken cancellationToken)
    {
        // Both Microsoft.Agents.AI and Microsoft.Extensions.AI ship a ChatClientExtensions type.
        // Use reflection to call the correct extension method without binding to a specific one.
        const string targetTypeName = "Microsoft.Extensions.AI.ChatClientExtensions";
        const string methodName = "GetStreamingResponseAsync";

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        Type? extType = assemblies
            .Select(a => a.GetType(targetTypeName, false))
            .FirstOrDefault(t => t is not null);

        extType ??= Type.GetType(targetTypeName + ", Microsoft.Extensions.AI.Abstractions", false);
        if (extType is null) throw new InvalidOperationException("Streaming extensions were not found.");

        MethodInfo? mi = extType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m =>
            {
                if (!string.Equals(m.Name, methodName, StringComparison.Ordinal)) return false;

                var p = m.GetParameters();
                return p.Length == 4;
            });

        return mi is null
            ? throw new InvalidOperationException("GetStreamingResponseAsync extension method was not found.")
            : (IAsyncEnumerable<ChatResponseUpdate>)mi.Invoke(null, [chatClient, input, options, cancellationToken])!;
    }








    public static ChatOptions? CreateChatOptionsWithTools(IReadOnlyList<object> tools)
    {
        ChatOptions options = new();

        // Attach tools via reflection so we don't take a compile-time dependency on the preview tool types.
        Type optionsType = options.GetType();
        PropertyInfo? toolsProp = optionsType.GetProperty("Tools")
                                  ?? optionsType.GetProperty("Tooling")
                                  ?? optionsType.GetProperty("Functions")
                                  ?? optionsType.GetProperty("FunctionTools");
        if (toolsProp is null || !toolsProp.CanWrite) return options;

        if (toolsProp.PropertyType.IsInstanceOfType(tools))
        {
            toolsProp.SetValue(options, tools);
            return options;
        }

        if (typeof(System.Collections.IList).IsAssignableFrom(toolsProp.PropertyType) &&
            Activator.CreateInstance(toolsProp.PropertyType) is System.Collections.IList list)
        {
            foreach (var t in tools) list.Add(t);

            toolsProp.SetValue(options, list);
        }

        return options;
    }








    public static Task<ChatResponse> GetResponseWithToolsAsync(
        IChatClient chatClient,
        IList<ChatMessage> messages,
        IReadOnlyList<object> tools,
        CancellationToken cancellationToken)
    {
        // Preferred: chatClient.GetResponseAsync(messages, options, ct)
        // Where options has a Tools/Functions/FunctionTools property.
        var methods = chatClient.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => string.Equals(m.Name, "GetResponseAsync", StringComparison.Ordinal))
            .ToArray();

        // Try overload with options
        MethodInfo? withOptions = methods.FirstOrDefault(m =>
        {
            var p = m.GetParameters();
            return p.Length == 3 && p[2].ParameterType == typeof(CancellationToken);
        });

        if (withOptions is null) return chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken);

        var options = CreateChatOptionsWithTools(tools) ?? TryCreateOptionsWithTools();
        return options is null
            ? chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken)
            : (Task<ChatResponse>)withOptions.Invoke(chatClient, [messages, options, cancellationToken])!;

        object? TryCreateOptionsWithTools()
        {
            // Look for Microsoft.Extensions.AI.ChatOptions or similar.
            Type optionsType = withOptions.GetParameters()[1].ParameterType;
            var optionsInstance = Activator.CreateInstance(optionsType);
            if (optionsInstance is null) return null;

            // Find a property that can accept a tool list.
            PropertyInfo? toolProp = optionsType.GetProperty("Tools")
                                     ?? optionsType.GetProperty("Tooling")
                                     ?? optionsType.GetProperty("Functions")
                                     ?? optionsType.GetProperty("FunctionTools");

            if (toolProp is null || !toolProp.CanWrite) return null;

            // We pass raw tool objects; the underlying client may support [Description]
            // and custom attributes. This keeps our app decoupled from preview types.
            if (toolProp.PropertyType.IsInstanceOfType(tools))
            {
                toolProp.SetValue(optionsInstance, tools);
                return optionsInstance;
            }

            // Try to build a compatible collection.
            if (typeof(System.Collections.IList).IsAssignableFrom(toolProp.PropertyType))
                if (Activator.CreateInstance(toolProp.PropertyType) is System.Collections.IList list)
                {
                    foreach (var t in tools) list.Add(t);

                    toolProp.SetValue(optionsInstance, list);
                    return optionsInstance;
                }

            return null;
        }
    }
}