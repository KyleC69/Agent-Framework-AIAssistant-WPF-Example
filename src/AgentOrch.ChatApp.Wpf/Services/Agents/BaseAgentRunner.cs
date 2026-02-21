using System.Collections.Specialized;
using System.ComponentModel;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;




namespace AgentOrch.ChatApp.Wpf.Agents;





public class BaseAgentRunner : ObservableObject, INotifyCollectionChanged, INotifyPropertyChanging
{


    public event NotifyCollectionChangedEventHandler? CollectionChanged;








    public async Task StartInteractiveChat(AIAgent aIAgent)
    {
        Console.WriteLine("\n=== Agent Framework with MCP Tools ===");
        Console.WriteLine("You can ask questions and I'll use the available MCP tools to help you.");
        Console.WriteLine("Type 'exit' to quit.\n");

        AgentSession agentThread = await aIAgent.CreateSessionAsync();
        var messages = new List<ChatMessage>();
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("User: ");
            Console.ResetColor();

            var userInput = Console.ReadLine();
            messages.Add(new ChatMessage(ChatRole.User, userInput));

            if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            try
            {

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Assistant: ");
                Console.ResetColor();

                agentThread = await aIAgent.CreateSessionAsync();

                // Run agent with user input and agent thread
                AgentResponse response = await aIAgent.RunAsync(messages, agentThread);
                messages.AddRange(response.Messages);

                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine();
            }
        }
    }








    public async Task StartInteractiveChat(Workflow workflow)
    {
        Console.WriteLine("\n=== Agent Framework with MCP Tools ===");
        Console.WriteLine("You can ask questions and I'll use the available MCP tools to help you.");
        Console.WriteLine("Type 'exit' to quit.\n");

        List<ChatMessage> messages = new();
        var userInput = string.Empty;

        while (true)
        {

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("User: ");
            Console.ResetColor();

            // Read user input
            userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            try
            {
                messages.Add(new ChatMessage(ChatRole.User, userInput));

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Assistant: ");
                Console.ResetColor();

                var result = string.Empty;

                // Execute workflow and process events
                await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages).ConfigureAwait(false);
                await run.TrySendMessageAsync(new TurnToken(true));

                List<ChatMessage> newMessages = new();
                await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
                    if (evt is AgentResponseUpdateEvent e)
                    {
                        // Console.WriteLine($"{e.ExecutorId}: {e.Data}");
                        Console.Write(e.Data);
                    }
                    else if (evt is WorkflowOutputEvent completed)
                    {
                        newMessages = (List<ChatMessage>)completed.Data!;
                        break;
                    }
                    else if (evt is ExecutorFailedEvent failed)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error: {failed.Data}");
                        Console.ResetColor();
                        Console.WriteLine();
                    }

                //  Console.WriteLine($"{evt.GetType().Name}: {evt.Data}");
                // Add new messages to conversation history
                messages.AddRange(newMessages.Skip(messages.Count));
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine();
            }
        }

        Console.WriteLine("Goodbye!");
        Console.ReadLine();
    }
}