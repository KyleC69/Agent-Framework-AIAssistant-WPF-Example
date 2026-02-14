namespace AgentOrch.ChatApp.Wpf.Services;


/*public static class ServiceCollectionExtensions
{
    private static IServiceCollection AddAgentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Microsoft.Extensions.AI chat client
        services.AddSingleton<IChatClient>(sp =>
        {
          var logger = sp.GetService<ILogger<OnnxChatClient>>();
            var modelDir = configuration["Onnx:ModelDirectory"]
                           ?? configuration["ONNX_MODEL_DIRECTORY"]
                           ?? configuration["OnnxModelDirectory"];

            if (string.IsNullOrWhiteSpace(modelDir))
                throw new InvalidOperationException(
                    "Missing ONNX model directory configuration. Set 'Onnx:ModelDirectory' in appsettings.json (or ONNX_MODEL_DIRECTORY env var)."
                );

            var systemPrompt = configuration["Onnx:SystemPrompt"]
                               ?? configuration["ONNX_SYSTEM_PROMPT"];

            var maxLength = configuration.GetValue("Onnx:MaxLength", 2048);
            var maxNewTokens = configuration.GetValue("Onnx:MaxNewTokens", 512);

            IChatClient client = new OnnxChatClient(modelDir, logger, systemPrompt, maxLength, maxNewTokens);
            return client;
        });


        // Workflow orchestrator
        services.AddSingleton<IAgentOrchestrator, WorkflowAgentOrchestrator>();

        return services;
    }





            */
/*

    // Back-compat for existing call-sites.
    public static IServiceCollection AddSemanticKernelInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        //   return services.AddAgentInfrastructure(configuration);
    }*/