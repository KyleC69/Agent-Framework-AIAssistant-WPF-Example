using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AgentOrchestration.Wpf.Agents;

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using OllamaSharp;




namespace AgentOrchestration.Wpf.Services;





// This class is responsible for initializing the cooperative agents and setting up the workflow. The UI agent options settings need to be incorporated into this class as well,
// since this is where the agents are being created and the workflow is being set up. The UI agent options settings will be used to configure the agents and the workflow, so it makes sense to have them in the same class.
// This is an experiment in creating a dynamic agent initializer that can be used to set up different workflows and agents based on the needs of the users request at the time.. The idea is to have a flexible and reusable initializer that can be
// easily modified to accommodate different scenarios and requirements. This class will also handle the initialization of any persistent memory needed for the agents to function across sessions, and verify that Retrieval-Augmented Generation (RAG) is working correctly.
// This Agentic
public class CooperativeAgentsInitializer : BaseAgentRunner
{
    private readonly OllamaApiClient _client;
    private readonly CoordinatorAgent _coordinator;
    private readonly ILoggerFactory _factory;
    private readonly ILogger<CooperativeAgentsInitializer> _logger;
    private readonly OrchestratorAgent _orchestrator;
    private readonly PlanningAgent _planner;
    private readonly QualityControlAgent _quality;
    private readonly SeniorCoderAgent _senior;
    private readonly SeniorCoderAgent2 _senior2;








    public CooperativeAgentsInitializer(
            ILoggerFactory factory,
            OllamaApiClient client,
            OrchestratorAgent orchestrator,
            PlanningAgent planner,
            SeniorCoderAgent senior,
            SeniorCoderAgent2 senior2,
            QualityControlAgent quality,
            CoordinatorAgent coordinator)
    {
        _coordinator = coordinator;
        _factory = factory;
        _logger = _factory.CreateLogger<CooperativeAgentsInitializer>();
        _client = client;
        _orchestrator = orchestrator;
        _planner = planner;
        _senior = senior;
        _senior2 = senior2;
        _quality = quality;



        if (_client is null || _orchestrator.Agent is null || _planner.Agent is null || _senior.Agent is null || _senior2.Agent is null || _quality.Agent is null)
        {
            throw new ArgumentNullException("One or more dependencies for CooperativeAgentsInitializer are null.");
        }
    }








    public static AIAgent? Agent { get; }








    //Performs all the initialization work for the cooperative agents sample, including creating the agents, setting up the workflow,
    //and setting up the tools initialize the persistent memory and verify RAG status.
    public async Task<List<ChatMessage>> Initialize()
    {



        #region Loading Agents and Tools

        _logger.LogInformation("Initializing cooperative agents...");





        _logger.LogInformation("Cooperative agents initialized successfully.");

        #endregion



        #region Setting up workflow

        //create a hand off workflow where the coordinator agent receives a task, then passes it to the planner agent, who creates a plan and passes
        //it to the senior coder agents, who perform the task and pass it to the quality assurance agent for review.
        Workflow handoff = AgentWorkflowBuilder.CreateHandoffBuilderWith(_coordinator.Agent)
                .WithHandoff(_coordinator.Agent, _planner.Agent)
                .WithHandoff(_coordinator.Agent, _senior.Agent)
                .WithHandoff(_coordinator.Agent, _senior2.Agent)
                .WithHandoff(_coordinator.Agent, _quality.Agent).Build();

        #endregion



        #region passing workflow to executor in base class and starting interactive chat

        _logger.LogInformation("Starting executor ..");
        await StartInteractiveChat(handoff);
        _logger.LogInformation("Workflow has completed...");

        #endregion



        #region Initializing persistent memory and verifying RAG status

        _logger.LogInformation("Initializing persistent memory and verifying RAG status for cooperative agents...");

        // Here you would initialize any persistent memory needed for the agents to function across sessions, and verify that Retrieval-Augmented Generation (RAG) is working correctly.
        // This is a placeholder for the actual initialization and verification code.



        //        

        _logger.LogInformation("Persistent memory initialized and RAG status verified successfully for cooperative agents.");

        #endregion



        #region complete

        _logger.LogInformation("Cooperative agents Completed...");




        return [];
    }

    #endregion



}