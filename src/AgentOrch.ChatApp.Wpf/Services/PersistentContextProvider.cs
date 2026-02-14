using System.Text.Json;
using System.Linq;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentOrch.ChatApp.Wpf.Services;

internal sealed class PersistentContextProvider : AIContextProvider
{
    private const int DefaultMaxEntries = 10;
    private readonly AIContextProvider? _innerProvider;
    private readonly JsonSerializerOptions? _jsonSerializerOptions;
    private readonly int _maxEntries;

    public PersistentContextProvider(AIContextProvider? innerProvider = null, int? maxEntries = null, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        _innerProvider = innerProvider;
        _maxEntries = maxEntries is > 0 ? maxEntries.Value : DefaultMaxEntries;
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    public PersistentContextProvider(JsonElement serializedState, AIContextProvider? innerProvider = null, JsonSerializerOptions? jsonSerializerOptions = null)
        : this(innerProvider, ReadMaxEntries(serializedState), jsonSerializerOptions)
    {
    }

    public async ValueTask<AIContext> InvokingAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>();
        AIContext? innerContext = null;

        if (_innerProvider is not null)
        {
            innerContext = await _innerProvider.InvokingAsync(context, cancellationToken);
            if (innerContext.Messages is not null)
            {
                messages.AddRange(innerContext.Messages);
            }
        }

        var entries = await ChatHistoryStorage.ReadContextEntriesAsync(_jsonSerializerOptions, cancellationToken);
        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry))
            {
                continue;
            }

            messages.Add(new ChatMessage(ChatRole.System, entry));
        }

        return innerContext is null
            ? new AIContext { Messages = messages }
            : new AIContext { Messages = messages, Tools = innerContext.Tools };
    }








    protected override ValueTask<AIContext> InvokingCoreAsync(InvokingContext context, CancellationToken cancellationToken = default)
        => InvokingAsync(context, cancellationToken);








    public async ValueTask InvokedAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (_innerProvider is not null)
        {
            await _innerProvider.InvokedAsync(context, cancellationToken);
        }

        var entries = context.ResponseMessages
            .Where(message => !string.IsNullOrWhiteSpace(message.Text))
            .TakeLast(_maxEntries)
            .Select(message => $"{message.Role}: {message.Text}")
            .ToList();

        await ChatHistoryStorage.WriteContextEntriesAsync(entries, _jsonSerializerOptions, cancellationToken);
    }

    public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        var state = new PersistentContextState { MaxEntries = _maxEntries };
        return JsonSerializer.SerializeToElement(state, jsonSerializerOptions ?? _jsonSerializerOptions);
    }

    private static int? ReadMaxEntries(JsonElement serializedState)
    {
        if (serializedState.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (serializedState.TryGetProperty("maxEntries", out var maxEntries) && maxEntries.TryGetInt32(out var value))
        {
            return value;
        }

        if (serializedState.TryGetProperty("MaxEntries", out maxEntries) && maxEntries.TryGetInt32(out value))
        {
            return value;
        }

        return null;
    }

    private sealed class PersistentContextState
    {
        public int MaxEntries { get; init; }
    }
}
