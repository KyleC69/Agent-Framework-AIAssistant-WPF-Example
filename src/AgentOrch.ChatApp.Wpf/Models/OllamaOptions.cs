namespace AgentOrchestration.Wpf.Models;





public sealed class OllamaOptions
{
    public string? Url { get; init; }

    public string? ModelId { get; init; }

    public bool EnableTelemetry { get; init; }
}