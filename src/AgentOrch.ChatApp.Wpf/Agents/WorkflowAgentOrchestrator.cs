using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using IChatClient = Microsoft.Extensions.AI.IChatClient;




namespace AgentOrchestration.Wpf.Agents;





public class WorkflowAgentOrchestrator
{



    public const int MaxIterations = 3;

    private readonly IWorkflowExecutionEnvironment _environment;
    private readonly ILogger<WorkflowAgentOrchestrator> _logger;
    private readonly SeniorCoderAgent coder1;
    private readonly SeniorCoderAgent2 coder2;
    private readonly OrchestratorAgent orchestrator;
    private readonly QualityControlAgent qaAgent;








    public WorkflowAgentOrchestrator(ILogger<WorkflowAgentOrchestrator> logger)
    {
        _logger = logger;



        _environment = App.GetRequiredService<IWorkflowExecutionEnvironment>();


        _ = App.GetRequiredService<IChatClient>();
        coder1 = App.GetRequiredService<SeniorCoderAgent>();
        coder2 = App.GetRequiredService<SeniorCoderAgent2>();
        orchestrator = App.GetRequiredService<OrchestratorAgent>();
        qaAgent = App.GetRequiredService<QualityControlAgent>();

    }








    private Workflow BuildGroupWorkflow()
    {
        IReadOnlyList<AIAgent> participants =
        [
                coder1.Agent,
                coder2.Agent,
                qaAgent.Agent,
                orchestrator.Agent
        ];

        GroupChatWorkflowBuilder builder = AgentWorkflowBuilder.CreateGroupChatBuilderWith(agents =>
        {
            RoundRobinGroupChatManager manager = new(agents)
            {
                MaximumIterationCount = MaxIterations
            };

            return manager;
        });

        return builder.AddParticipants(participants).Build();
    }








    public async IAsyncEnumerable<string> RunGroupWorkflowStreamingAsync(string input, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Workflow workflow = BuildGroupWorkflow();
        _ = new ChatMessage(ChatRole.User, input);

        await using StreamingRun run = await InProcessExecution.OpenStreamingAsync(workflow);
        bool started = await run.TrySendMessageAsync(new TurnToken(true));
        if (!started)
        {
            _logger.LogError("Failed to start the workflow.");
            yield break;
        }


        await foreach (WorkflowEvent workflowEvent in run.WatchStreamAsync(cancellationToken))
        {
            if (workflowEvent is AgentResponseUpdateEvent updateEvent)
            {
                AgentResponseUpdate update = updateEvent.Update;
                if (!string.IsNullOrWhiteSpace(update.Text))
                {
                    yield return update.Text;
                }

                continue;
            }

            if (workflowEvent is AgentResponseEvent responseEvent)
            {
                foreach (ChatMessage message in responseEvent.Response.Messages)
                {
                    if (!string.IsNullOrWhiteSpace(message.Text))
                    {
                        yield return message.Text;
                    }
                }

                continue;
            }

            if (workflowEvent is WorkflowOutputEvent outputEvent && outputEvent.Is(out ChatMessage? outputMessage))
            {
                if (!string.IsNullOrWhiteSpace(outputMessage.Text))
                {
                    yield return outputMessage.Text;
                }

                continue;
            }

            if (workflowEvent is WorkflowOutputEvent textOutput && textOutput.Is(out string? text))
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    yield return text;
                }
            }
            else if (workflowEvent is WorkflowErrorEvent errorEvent && errorEvent.Exception is not null)
            {
                _logger.LogError(errorEvent.Exception, "Workflow execution failed.");
            }
        }
    }
}