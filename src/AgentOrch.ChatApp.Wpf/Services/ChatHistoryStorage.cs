using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.AI;




namespace AgentOrchestration.Wpf.Services;





internal static class ChatHistoryStorage
{
    private const string HistoryFolderName = "AgentOrch\\ChatHistory";
    private const string ContextFileName = "context.json";
    private const string HistoryFileExtension = ".json";








    public static string GetHistoryDirectory()
    {
        string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string historyPath = Path.Combine(basePath, HistoryFolderName);
        _ = Directory.CreateDirectory(historyPath);
        return historyPath;
    }








    public static string GetHistoryFilePath(string threadId)
    {
        return Path.Combine(GetHistoryDirectory(), $"{threadId}.json");
    }








    public static string GetContextFilePath()
    {
        return Path.Combine(GetHistoryDirectory(), ContextFileName);
    }








    public static async Task<IReadOnlyList<ChatMessage>> ReadHistoryAsync(string threadId, JsonSerializerOptions? options, CancellationToken cancellationToken)
    {
        string path = GetHistoryFilePath(threadId);
        if (!File.Exists(path))
        {
            return [];
        }

        await using FileStream stream = File.OpenRead(path);
        var messages = await JsonSerializer.DeserializeAsync<List<ChatMessage>>(stream, options, cancellationToken);
        return messages ?? [];
    }








    public static async Task WriteHistoryAsync(string threadId, IEnumerable<ChatMessage>? messages, JsonSerializerOptions? options, CancellationToken cancellationToken)
    {
        string path = GetHistoryFilePath(threadId);
        await using FileStream stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, messages, options, cancellationToken);
    }








    public static async Task WriteContextEntriesAsync(IEnumerable<string> entries, JsonSerializerOptions? options, CancellationToken cancellationToken)
    {
        string path = GetContextFilePath();
        await using FileStream stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, entries, options, cancellationToken);
    }








    public static string? TryGetLatestHistoryFilePath()
    {
        string historyPath = GetHistoryDirectory();
        FileInfo? latest = Directory.EnumerateFiles(historyPath, "*" + HistoryFileExtension)
                .Where(path => !path.EndsWith(ContextFileName, StringComparison.OrdinalIgnoreCase))
                .Select(path => new FileInfo(path))
                .OrderByDescending(info => info.LastWriteTimeUtc)
                .FirstOrDefault();

        return latest?.FullName;
    }








    public static async Task<IReadOnlyList<ChatMessage>> ReadLatestHistoryAsync(JsonSerializerOptions? options, CancellationToken cancellationToken)
    {
        string? latestPath = TryGetLatestHistoryFilePath();
        if (string.IsNullOrWhiteSpace(latestPath) || !File.Exists(latestPath))
        {
            return [];
        }

        await using FileStream stream = File.OpenRead(latestPath);
        var messages = await JsonSerializer.DeserializeAsync<List<ChatMessage>>(stream, options, cancellationToken);
        return messages ?? [];
    }








    public static async Task<IReadOnlyList<string>> ReadContextEntriesAsync(JsonSerializerOptions? options, CancellationToken cancellationToken)
    {
        string path = GetContextFilePath();
        if (!File.Exists(path))
        {
            return [];
        }

        await using FileStream stream = File.OpenRead(path);
        var entries = await JsonSerializer.DeserializeAsync<List<string>>(stream, options, cancellationToken);
        return entries ?? [];
    }
}