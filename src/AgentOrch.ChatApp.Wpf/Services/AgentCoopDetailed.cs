using System.Text.Json;

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using OllamaSharp;




namespace AgentOrch.ChatApp.Wpf.Services;





//Class is representative of a multi-agent cooperative task, where multiple agents work together to achieve a common goal. It contains detailed information about the cooperative task, such as the agents involved, their roles, the task description, and any relevant metadata.
//This class can be used to manage and track the progress of cooperative tasks within the application.
//This is generally used for detailed tasks or when a high-level of accuracy is desired to complete the task.
internal class AgentCoopDetailed
{
    private readonly ILoggerFactory _factory;








    public AgentCoopDetailed(ILoggerFactory factory)
    {
        _factory = factory;
        // Initialize properties or perform any setup required for the cooperative task.
    }








    public async Task<List<ChatMessage>> BuildAgentCoopDetailedAsync(string task)
    {






        // 1. Create agents
        AIAgent plannerAgent = CreateAgent(
                "You are a senior programmer and will act as the planner and coordinator for a group of agents. " +
                "You will create the step-by-step plan for the tasks you are given. It is your job to ensure that " +
                "all tasks are completed efficiently and accurately according to the users requirements.");

        //   AgentResponse results = await plannerAgent.RunAsync(task);



        AIAgent workingAgent = CreateAgent(
                "You are a junior programmer and will act as the worker for a group of agents. " +
                "You will receive tasks from the planner and execute them to the best of your ability. " +
                "You may use tools that are available to you to complete the tasks.");

        AIAgent codeReviewAgent = CreateAgent(
                "You are a senior programmer and will act as the code reviewer for a group of agents. " +
                "You will receive completed tasks from the worker and review them for accuracy and quality. " +
                "You will provide feedback to the worker to help them improve their work. If necessary, you may " +
                "suggest alternative approaches or solutions and send them back for revision. It is your primary " +
                "task to ensure that the work meets the required standards and that the final output is of high quality " +
                "and completes the steps outlined in the plan.");

        // 2. Build workflow
        Workflow workflow = AgentWorkflowBuilder
                .CreateHandoffBuilderWith(plannerAgent)
                .WithHandoffs(plannerAgent, [workingAgent])
                .WithHandoffs([workingAgent], codeReviewAgent)
                .Build();

        // 3. Initial messages
        List<ChatMessage> messages = [];
        messages.Add(new ChatMessage(ChatRole.User, task));

        // 4. Run workflow (streaming)

        var dotstring = workflow.ToDotString();


        await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);
        await run.TrySendMessageAsync(new TurnToken(true));
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
            switch (evt)
            {
                case ExecutorInvokedEvent invoke:
                    Console.WriteLine($"Starting {invoke.ExecutorId}");
                    break;

                case ExecutorCompletedEvent complete:
                    Console.WriteLine($"Completed {complete.ExecutorId}: {complete.Data}");
                    break;

                case WorkflowOutputEvent output:
                    Console.WriteLine($"Workflow output: {output.Data}");
                    return messages;

                case WorkflowErrorEvent error:
                    Console.WriteLine($"Workflow error: {error.Exception}");
                    return messages;

                default:
                    Console.WriteLine($"Unknown workflow event: {evt}");
                    break;
            }

        return messages;
    }








    private static async Task<List<ChatMessage>> RunWorkflowStreamingAsync(
            Workflow workflow,
            string task)
    {
        string? lastExecutorId = null;

        // 1. Start workflow with NO initial messages
        await using StreamingRun run = await InProcessExecution.OpenStreamAsync(workflow);

        // 2. Send the user message into the workflow
        await run.TrySendMessageAsync(new ChatMessage(ChatRole.User, task));

        // 3. Signal that the turn should advance
        await run.TrySendMessageAsync(new TurnToken(true));

        // 4. Read events from the workflow
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
            if (evt is AgentResponseUpdateEvent e)
            {
                if (e.ExecutorId != lastExecutorId)
                {
                    lastExecutorId = e.ExecutorId;
                    Console.WriteLine();
                    Console.WriteLine(e.ExecutorId);
                }

                Console.Write(e.Update.Text);

                if (e.Update.Contents.OfType<FunctionCallContent>().FirstOrDefault() is FunctionCallContent call)
                {
                    Console.WriteLine();
                    Console.WriteLine(
                            $"  [Calling function '{call.Name}' with arguments: {JsonSerializer.Serialize(call.Arguments)}]");
                }
            }
            else if (evt is WorkflowOutputEvent output)
            {
                Console.WriteLine();
                return output.As<List<ChatMessage>>()!;
            }

        return [];
    }








    private static ChatClientAgent CreateAgent(string instructions)
    {
        //Llamma is and has a small footprint, making it fast for local use.
        OllamaApiClient client = new(new Uri("http://localhost:11434"), "llama3.2:1b");
        return new ChatClientAgent(client, new ChatClientAgentOptions
        {
                ChatOptions = new ChatOptions { Instructions = instructions }
                //      ChatHistoryProviderFactory = (ctx, ct) => new ValueTask<ChatHistoryProvider>(new PersistentChatHistoryProvider(ctx.SerializedState, ctx.JsonSerializerOptions)),
                //        AIContextProviderFactory = (ctx, ct) => new ValueTask<AIContextProvider>(new PersistentContextProvider(jsonSerializerOptions: ctx.JsonSerializerOptions))
        });
    }
}





public static class LocalModels
{
    public const string llama3_2_1b = "llama3.2:1b";
    public const string bge_large_latest = "bge-large:latest";
    public const string llama3_2_3b = "llama3.2:3b";
    public const string qwen2_5_code_1_5b_base = "qwen2.5-coder:1.5b-base";
    public const string deepseek_coder_6_7b = "deepseek-coder:6.7b";
    public const string LFM2_5_function_calling_latest = "LFM2.5-function-calling:latest";
}