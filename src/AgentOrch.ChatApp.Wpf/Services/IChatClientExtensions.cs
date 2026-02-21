using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.AI;




namespace AgentOrchestration.Wpf.Services;





public static class IChatClientExtensions
{
    public static async IAsyncEnumerable<string> InferStreaming(this IChatClient client, string system, string prompt, [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (ChatResponseUpdate messagePart in client.GetStreamingResponseAsync(
                               [
                                       new ChatMessage(ChatRole.System, system),
                                       new ChatMessage(ChatRole.User, prompt)
                               ],
                               null,
                               ct))
        {
            yield return messagePart.Text ?? string.Empty;
        }
    }








    public static async Task<bool> ValidateConfiguredModelAsync(this IChatClient client, CancellationToken ct = default)
    {
        ChatResponse response = await client.GetResponseAsync(
                [
                        new ChatMessage(ChatRole.System, "You are a model health check assistant."),
                        new ChatMessage(ChatRole.User, "Respond with OK.")
                ],
                null,
                ct);

        string text = response.Text;
        if (!string.IsNullOrWhiteSpace(text))
        {
            if (text.Trim().Equals("OK", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        else
        {
            return false;

        }

        throw new InvalidOperationException($"Configured model did not return expected response. Actual response: {text}");
    }








    public static IAsyncEnumerable<string> SummarizeTextAsync(this IChatClient client, string userText, CancellationToken ct = default)
    {
        return client.InferStreaming("", $"Summarize this text in three to five bullet points:\r {userText}", ct);
    }








    public static IAsyncEnumerable<string> FixAndCleanUpTextAsync(this IChatClient client, string userText, CancellationToken ct = default)
    {
        string systemMessage = "Your job is to fix spelling, and clean up the text from the user. Only respond with the updated text. Do not explain anything.";
        return client.InferStreaming(systemMessage, userText, ct);
    }








    public static IAsyncEnumerable<string> AutocompleteSentenceAsync(this IChatClient client, string sentence, CancellationToken ct = default)
    {
        string systemMessage = "You are an assistant that helps the user complete sentences. Ignore spelling mistakes and just respond with the words that complete the sentence. Do not repeat the begining of the sentence.";
        return client.InferStreaming(systemMessage, sentence, ct);
    }








    public static Task<List<string>> GetTodoItemsFromText(this IChatClient client, string text)
    {
        return Task.Run(async () =>
        {
            string system = "Summarize the user text to 2-3 to-do items. Use the format [\"to-do 1\", \"to-do 2\"]. Respond only in one json array format";
            string response = string.Empty;

            CancellationTokenSource cts = new();
            await foreach (string partialResult in client.InferStreaming(system, text, cts.Token))
            {
                response += partialResult;
                if (partialResult.Contains("]"))
                {
                    cts.Cancel();
                    break;
                }
            }

            var todos = Regex.Matches(response, @"""([^""]*)""", RegexOptions.IgnoreCase | RegexOptions.Multiline)
                    .Select(m => m.Groups[1].Value)
                    .Where(t => !t.Contains("todo")).ToList();

            return todos;
        });
    }








    public static IAsyncEnumerable<string> AskForContentAsync(this IChatClient client, string content, string question, CancellationToken ct = default)
    {
        string systemMessage = "You are a helpful assistant answering questions about this content";
        return client.InferStreaming($"{systemMessage}: {content}", question, ct);
    }
}