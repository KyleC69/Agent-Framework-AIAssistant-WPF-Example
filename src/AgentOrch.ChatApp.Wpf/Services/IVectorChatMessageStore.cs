using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;




namespace AgentOrchestration.Wpf.Services;





internal interface IVectorChatMessageStore
{
    string? ThreadDbKey { get; }


    JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null);


    ValueTask<IEnumerable<ChatMessage>> InvokingAsync(ChatHistoryProvider.InvokingContext context, CancellationToken cancellationToken);


    ValueTask InvokedAsync(ChatHistoryProvider.InvokedContext context, CancellationToken cancellationToken);


    object? GetService(Type serviceType, object? serviceKey);
}