using AgentOrch.ChatApp.Wpf.Models;



namespace AgentOrch.ChatApp.Wpf.Services.Agents;


public static class Definitions
{
    private const string TeamLeaderInstructions =
        """
        You are the leader of a programming team and your job is to oversee the work of your team members.
        The goal is to deliver the best and accurate example of the code requested by the user.
        You need to ensure the code is well structured, efficient, and follows best practices and is accurate.
        Code must use existing types and avoid manual implementations where possible. Code must not contain placeholders or stubs.
        If so, state that it is approved.
        If not, provide insight on how to refine suggested copy without example.
        """;

    private const string ProgrammerName = "Programmer";

    private const string ProgrammerInstructions =
        """
        You are a senior programmer with ten years of experience and are known for writing well
        structured and deterministic code. Make no assumptions, always provide complete code examples, no stubs or placeholders.
        The goal is to create the code requested by the user as an expert in the field. The code must be
        well structured, efficient, and follow best practices. It should also use existing types and avoid
        manual implementations where possible.
        Only provide a single proposal per response.
        You're laser focused on the goal at hand.
        Consider suggestions when refining an idea.
        """;

    /*
    private static SKModelConnection gitConn = new SKModelConnection
    {
        Type = "GitHub",
        Endpoint = TestConfiguration.OpenAI.Endpoint,
        ApiKey = TestConfiguration.OpenAI.ApiKey,
        Model = TestConfiguration.OpenAI.ChatModelId,
        ServiceId = "AgentOrch",
        ExtensionData = new Dictionary<string, object?>()
    };
    */
    private static readonly SkModelConnection LocalConn = new()
    {
        Type = "Phi-3",
        Endpoint = @"""F:\AI-Models\cpu-int4-rtn-block-32""",
        Model = "Phi3",
        ServiceId = "AgentOrch",
        ExtensionData = new Dictionary<string, object?>()
    };

    /*
        private static SKModelDefinition git = new SKModelDefinition
        {
            Id = "Llama-3.3-8b",
            Api = "Github",
            Connection = gitConn,
            Type = null,
            Options = new Dictionary<string, object>
            {
                {
                    "temperature", 0.7f
                },
                {
                    "max_tokens", 2048
                },
            }
        };
    */
    private static readonly SkModelDefinition Local = new()
    {
        Id = "Phi-3",
        Api = "Local",
        Connection = LocalConn,
        Type = "local",
        Options = new Dictionary<string, object> { { "temperature", 0.7f }, { "max_tokens", 2048 } }
    };


    public static readonly SkAgentDefinition LeaderAgent = new()
    {
        Version = "1.1",
        Id = "prog1",
        Type = "ChatAgent",
        Name = "Leader",
        Description = "This agent acts as a team leader",
        Instructions = TeamLeaderInstructions,
        Metadata = null,
        Model = Local,
        Inputs = null,
        Outputs = null,
        Template = null,
        Tools = null
    };


    public static SkAgentDefinition ProgrammerAgent = new()
    {
        Version = "1.1",
        Id = "prog1",
        Type = "ChatAgent",
        Name = "Programmer",
        Description = "This agent acts as a team leader",
        Instructions = TeamLeaderInstructions,
        Metadata = null,
        Model = Local,
        Inputs = null,
        Outputs = null,
        Template = null,
        Tools = null
    };
}