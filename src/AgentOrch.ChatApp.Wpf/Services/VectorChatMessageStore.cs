using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;




namespace AgentOrchestration.Wpf.Services;





internal sealed class VectorChatMessageStore : ChatHistoryProvider, IVectorChatMessageStore
{
    private readonly VectorStore _vectorStore;








    public VectorChatMessageStore(
            VectorStore vectorStore,
            JsonElement serializedStoreState,
            JsonSerializerOptions? jsonSerializerOptions = null)
    {
        _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));
        if (serializedStoreState.ValueKind is JsonValueKind.String)
        {
            ThreadDbKey = serializedStoreState.Deserialize<string>(jsonSerializerOptions);
        }
    }








    public string? ThreadDbKey { get; private set; }








    public JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        // We have to serialize the thread id, so that on deserialization you can retrieve the messages using the same thread id.
        return JsonSerializer.SerializeToElement(ThreadDbKey, jsonSerializerOptions);
    }








    private async Task SaveMessagesAsync(
            IEnumerable<ChatMessage> messages,
            CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(messages);

        ThreadDbKey ??= Guid.NewGuid().ToString("N");

        var collection = _vectorStore.GetCollection<string, ChatHistoryItem>("ChatHistory");
        await collection.EnsureCollectionExistsAsync(cancellationToken);

        DateTimeOffset now = DateTimeOffset.UtcNow;
        await collection.UpsertAsync(
                messages.Select(x => new ChatHistoryItem
                {
                        Key = CreateItemKey(ThreadDbKey!, x),
                        Timestamp = now,
                        ThreadId = ThreadDbKey,
                        SerializedMessage = JsonSerializer.Serialize(x),
                        MessageText = x.Text
                }),
                cancellationToken);
    }








    private async Task<IEnumerable<ChatMessage>> LoadMessagesAsync(
            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ThreadDbKey))
        {
            return [];
        }

        var collection = _vectorStore.GetCollection<string, ChatHistoryItem>("ChatHistory");
        await collection.EnsureCollectionExistsAsync(cancellationToken);
        var records = collection
                .GetAsync(
                        x => x.ThreadId == ThreadDbKey, 100,
                        new FilteredRecordRetrievalOptions<ChatHistoryItem> { OrderBy = x => x.Descending(y => y.Timestamp) },
                        cancellationToken);

        List<ChatMessage> messages = [];
        await foreach (ChatHistoryItem record in records)
        {
            if (string.IsNullOrWhiteSpace(record.SerializedMessage))
            {
                continue;
            }

            if (JsonSerializer.Deserialize<ChatMessage>(record.SerializedMessage) is { } message)
            {
                messages.Add(message);
            }
        }

        messages.Reverse();
        return messages;
    }








    protected override ValueTask<IEnumerable<ChatMessage>> InvokingCoreAsync(
            InvokingContext context,
            CancellationToken cancellationToken = default)
    {
        return new ValueTask<IEnumerable<ChatMessage>>(LoadMessagesAsync(cancellationToken));
    }








    protected override async ValueTask InvokedCoreAsync(
            InvokedContext context,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        await SaveMessagesAsync(context.ResponseMessages, cancellationToken);
    }








    private static string CreateItemKey(string threadDbKey, ChatMessage message)
    {
        // MessageId may be null/empty depending on the provider; ensure stable uniqueness per thread.
        var messageId = string.IsNullOrWhiteSpace(message.MessageId) ? Guid.NewGuid().ToString("N") : message.MessageId;
        return $"{threadDbKey}:{messageId}";
    }








    private sealed class ChatHistoryItem
    {
        [VectorStoreKey] public string? Key { get; set; }

        [VectorStoreData] public string? ThreadId { get; set; }

        [VectorStoreData] public DateTimeOffset? Timestamp { get; set; }

        [VectorStoreData] public string? SerializedMessage { get; set; }

        [VectorStoreData] public string? MessageText { get; set; }
    }
}