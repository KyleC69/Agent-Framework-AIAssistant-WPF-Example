using System.Diagnostics.CodeAnalysis;



namespace AgentOrch.ChatApp.Wpf.Models;


/// <summary>
///     Provides a definition for an agent, including its version, ID, type, name, description, instructions, metadata,
///     model, inputs, outputs, template options, and tools.
/// </summary>
[Experimental("SKEXP0110")]
public class SkAgentDefinition
{
    /// <summary>
    ///     Gets or sets the version of the agent.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier of the agent.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    ///     Gets or sets the type of the  agent.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    ///     Gets or sets the name of the  agent.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the short description of the agent.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the instructions for the agent to use.
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    ///     Gets or sets the metadata associated with the agent, including its authors and tags
    ///     as specific metadata but can accept any optional metadata that can be handled by the provider.
    /// </summary>
    public object? Metadata { get; set; }

    /// <summary>
    ///     Gets or sets the model used by the agent, including the API, connection, and options.
    /// </summary>
    public SkModelDefinition? Model { get; set; }

    /// <summary>
    ///     Gets or sets the collection of inputs used by the agent, including their type, default value, and description.
    /// </summary>
    /// <remarks>
    ///     This is typically a set of inputs that will be used as parameters that participate in the template rendering.
    /// </remarks>
    public IDictionary<string, object>? Inputs { get; set; }

    /// <summary>
    ///     Gets or sets the collection of outputs supported by the agent, including their type and description.
    /// </summary>
    public IDictionary<string, object>? Outputs { get; set; }

    /// <summary>
    ///     Gets or sets the template options used by the agent, including its type and parser.
    /// </summary>
    public object? Template { get; set; }

    /// <summary>
    ///     Gets or sets the collection of tools used by the agent, including their name, type, and options.
    /// </summary>
    public IList<object>? Tools { get; set; }
}