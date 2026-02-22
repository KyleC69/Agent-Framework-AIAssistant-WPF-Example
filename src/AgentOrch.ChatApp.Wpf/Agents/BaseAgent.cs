using Microsoft.Agents.AI;




namespace AgentOrchestration.Wpf.Agents;





public class BaseAgent
{
    public required ChatClientAgent _agent;





    public AIAgent Agent => _agent;
}