// ReSharper disable UnusedMember.Global // Methods aren't instantiated by user code but are necessary for reflection



using System.ComponentModel;
using System.IO;
using System.Text;

using Markdig;



namespace AgentOrch.ChatApp.Wpf.ToolFunctions;


//this is to format code snippets and prepare them for final output ??? possible?
public sealed class CodeWriterPlugin
{

    [Description("The final output of any generated code should pass through here.")]
    public string Generate([Description("Describe the program or function to generate")] string description)
    {
        return description;
    }
}





/// <summary>
///     Provides functionality to format content as Markdown for final output.
/// </summary>
public class OutputMarkDownPlugin
{

    [Description("Formats the final output as markdown.")]
    public string FormatAsMarkdownAsync([Description("The content to format as markdown")] string content)
    {
        return ConvertModelOutputToMarkdown(content);
    }








    public static string ConvertModelOutputToMarkdown(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return string.Empty;

        // Remove common local-model stop tokens that can leak into output.
        var cleaned = content
            .Replace("<|end|>", string.Empty, StringComparison.Ordinal)
            .Replace("<|user|>", string.Empty, StringComparison.Ordinal)
            .Replace("<|system|>", string.Empty, StringComparison.Ordinal)
            .Trim();

        cleaned = NormalizeCodeFences(cleaned);

        // Validate markdown via Markdig parse so obvious issues surface during development.
        // Keep return type as markdown string because the tool API expects string.
        MarkdownPipeline? pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        _ = Markdown.Parse(cleaned, pipeline);
        return cleaned;
    }








    private static string NormalizeCodeFences(string text)
    {
        // Ensure fenced blocks are on their own lines so renderers (Markdig.Wpf, GitHub, etc.) behave consistently.
        StringBuilder sb = new(text.Length + 32);
        using StringReader sr = new(text);
        string? line;
        while ((line = sr.ReadLine()) is not null)
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("```", StringComparison.Ordinal) && line.Length != trimmed.Length)
                sb.Append(trimmed);
            else
                sb.Append(line);

            sb.Append('\n');
        }

        return sb.ToString();
    }
}