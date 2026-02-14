using Microsoft.Agents.AI.Workflows;




namespace AgentOrch.ChatApp.Wpf;





internal sealed class WordCountingExecutor() : Executor<string, FileStats>("WordCountingExecutor")
{
    public override async ValueTask<FileStats> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // Retrieve the file content from the shared state
        var fileContent = await context.ReadStateAsync<string>(message, FileContentStateConstants.FileContentStateScope, cancellationToken)
                          ?? throw new InvalidOperationException("File content state not found");

        var wordCount = fileContent.Split([' ', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Length;

        return new FileStats { WordCount = wordCount };
    }
}





internal sealed class ParagraphCountingExecutor() : Executor<string, FileStats>("ParagraphCountingExecutor")
{
    public override async ValueTask<FileStats> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // Retrieve the file content from the shared state
        var fileContent = await context.ReadStateAsync<string>(message, FileContentStateConstants.FileContentStateScope, cancellationToken)
                          ?? throw new InvalidOperationException("File content state not found");

        var paragraphCount = fileContent.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Length;

        return new FileStats { ParagraphCount = paragraphCount };
    }
}





internal sealed class AggregationExecutor() : Executor<FileStats>("AggregationExecutor")
{
    private readonly List<FileStats> _messages = [];








    public override async ValueTask HandleAsync(FileStats message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        _messages.Add(message);

        if (_messages.Count == 2)
        {
            // Aggregate the results from both executors
            var totalParagraphCount = _messages.Sum(m => m.ParagraphCount);
            var totalWordCount = _messages.Sum(m => m.WordCount);
            await context.YieldOutputAsync($"Total Paragraphs: {totalParagraphCount}, Total Words: {totalWordCount}", cancellationToken);
        }
    }
}





internal sealed class FileStats
{
    public int ParagraphCount { get; set; }
    public int WordCount { get; set; }
}





/// <summary>
///     Constants for shared state scopes.
/// </summary>
internal static class FileContentStateConstants
{
    public const string FileContentStateScope = "FileContentState";
}





internal sealed class FileReadExecutor() : Executor<string, string>("FileReadExecutor")
{
    public override async ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // Read file content from embedded resource
        var fileContent = Resources.Read(message);
        // Store file content in a shared state for access by other executors
        var fileID = Guid.NewGuid().ToString("N");
        await context.QueueStateUpdateAsync(fileID, fileContent, FileContentStateConstants.FileContentStateScope, cancellationToken);

        return fileID;
    }
}





internal class Resources
{
    public static string Read(string message)
    {
        throw new NotImplementedException();
    }
}