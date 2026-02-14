using System.Text.Json;
using System.IO;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentOrch.ChatApp.Wpf.Services;

internal sealed class PersistentChatHistoryProvider : ChatHistoryProvider
{
    private readonly JsonSerializerOptions? _jsonSerializerOptions;

    public PersistentChatHistoryProvider(string? threadId = null, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        ThreadId = threadId;
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    public PersistentChatHistoryProvider(JsonElement serializedState, JsonSerializerOptions? jsonSerializerOptions = null)
        : this(serializedState.ValueKind == JsonValueKind.String
            ? serializedState.Deserialize<string>(jsonSerializerOptions)
            : null, jsonSerializerOptions)
    {
    }

    public string? ThreadId { get; private set; }

    public async ValueTask<IEnumerable<ChatMessage>> InvokingAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        EnsureThreadIdFromLatest();

        if (string.IsNullOrWhiteSpace(ThreadId))
        {
            return [];
        }

        return await ChatHistoryStorage.ReadHistoryAsync(ThreadId, _jsonSerializerOptions, cancellationToken);
    }








    protected override ValueTask<IEnumerable<ChatMessage>> InvokingCoreAsync(InvokingContext context, CancellationToken cancellationToken = default)
        => InvokingAsync(context, cancellationToken);








    public async ValueTask InvokedAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        ThreadId ??= Guid.NewGuid().ToString("N");
        await ChatHistoryStorage.WriteHistoryAsync(ThreadId, context.ResponseMessages, _jsonSerializerOptions, cancellationToken);
    }








    protected override ValueTask InvokedCoreAsync(InvokedContext context, CancellationToken cancellationToken = default)
        => InvokedAsync(context, cancellationToken);








    public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return JsonSerializer.SerializeToElement(ThreadId, jsonSerializerOptions ?? _jsonSerializerOptions);
    }

    private void EnsureThreadIdFromLatest()
    {
        if (!string.IsNullOrWhiteSpace(ThreadId))
        {
            return;
        }

        var latestPath = ChatHistoryStorage.TryGetLatestHistoryFilePath();
        if (string.IsNullOrWhiteSpace(latestPath))
        {
            return;
        }

        ThreadId = Path.GetFileNameWithoutExtension(latestPath);
    }
}
