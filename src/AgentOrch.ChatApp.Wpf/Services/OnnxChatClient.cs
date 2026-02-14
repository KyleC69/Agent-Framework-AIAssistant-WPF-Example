using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.Extensions.AI;
using Microsoft.ML.OnnxRuntimeGenAI;



namespace AgentOrch.ChatApp.Wpf.Services;


public sealed class OnnxChatClient : IChatClient, IDisposable, IAsyncDisposable
{
    private readonly string _modelPath;
    private readonly int _outputTokens;
    private readonly string? _systemPrompt;
    private readonly Tokenizer? _tokenizer;
    private bool _disposed;

    private Model? _model;








    public OnnxChatClient(
        string modelPath,
        string? systemPrompt = null,
        int outputTokens = 512)
    {
        if (string.IsNullOrWhiteSpace(modelPath))
        {
            throw new ArgumentException("Model path must be provided.", nameof(modelPath));
        }

        _modelPath = modelPath;

        _systemPrompt = systemPrompt;
        _outputTokens = outputTokens;



        _model = new Model(_modelPath);
        _tokenizer = new Tokenizer(_model);
    }








    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }








    // ------------------ Non‑streaming ------------------








    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messages);
        ThrowIfDisposed();

        StringBuilder sb = new();

        await foreach (ChatResponseUpdate update in GetStreamingResponseAsync(messages, options, cancellationToken)
                           .ConfigureAwait(false))
            if (!string.IsNullOrEmpty(update.Text))
                sb.Append(update.Text);

        return new ChatResponse(new ChatMessage(ChatRole.Assistant, sb.ToString()));
    }








    // ------------------ Streaming ------------------








    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messages);
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();

        EnsureLoaded();

        var prompt = BuildPrompt(messages, _systemPrompt);

        using Sequences? sequences = _tokenizer!.Encode(prompt);
        if (sequences is null || sequences.NumSequences == 0)
            throw new InvalidOperationException("Tokenizer returned no sequences for the prompt.");

        var promptTokens = sequences[0].Length;
        var maxLength = promptTokens + _outputTokens;

        GeneratorParams genParams = new(_model!);
        genParams.SetSearchOption("max_length", maxLength);
        genParams.SetSearchOption("temperature", options?.Temperature ?? 0.7);
        genParams.SetSearchOption("top_p", options?.TopP ?? 0.9);
        genParams.SetSearchOption("do_sample", true);

        using TokenizerStream? stream = _tokenizer.CreateStream();
        using Generator generator = new(_model!, genParams);

        generator.AppendTokenSequences(sequences);

        while (!generator.IsDone())
        {
            cancellationToken.ThrowIfCancellationRequested();

            generator.GenerateNextToken();
            var seq = generator.GetSequence(0);
            if (seq.Length == 0)
            {
                await Task.Yield();
                continue;
            }

            var part = stream.Decode(seq[^1]);
            if (string.IsNullOrEmpty(part))
            {
                await Task.Yield();
                continue;
            }

            yield return new ChatResponseUpdate(ChatRole.Assistant, part);

            await Task.Yield();
        }
    }








    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return App.Services?.GetService(serviceType);
    }








    // ------------------ Dispose ------------------








    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _tokenizer?.Dispose();
        _model?.Dispose();
    }








    // ------------------ Required by IChatClient ------------------








    public TService? GetService<TService>() where TService : class
    {
        return null;
    }








    // ------------------ Helpers ------------------








    private void EnsureLoaded()
    {
        if (_model is null)
            _model = new Model(_modelPath);

        if (_tokenizer is null) throw new InvalidOperationException("Tokenizer is not initialized.");
    }








    private static string BuildPrompt(IEnumerable<ChatMessage> messages, string? systemPrompt)
    {
        StringBuilder sb = new();

        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            sb.AppendLine(systemPrompt);
            sb.AppendLine();
        }

        foreach (ChatMessage m in messages)
        {
            var role =
                m.Role == ChatRole.User ? "user" :
                m.Role == ChatRole.Assistant ? "assistant" :
                m.Role == ChatRole.System ? "system" :
                "user";

            sb.AppendLine($"{role}: {m.Text}");
        }

        sb.Append("assistant: ");
        return sb.ToString();
    }








    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(OnnxChatClient));
    }
}