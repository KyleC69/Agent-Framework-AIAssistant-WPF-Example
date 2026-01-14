using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntimeGenAI;



public sealed class OnnxChatClient : IChatClient
{
    private readonly ILogger<OnnxChatClient>? _logger;
    private readonly ConcurrentDictionary<(Type type, object? key), object?> _services = new();

    private readonly string _modelDirectory;
    private readonly string _systemPrompt;
    private readonly int _maxLength;
    private readonly int _maxNewTokens;

    private readonly object _gate = new();
    private Model? _model;
    private Tokenizer? _tokenizer;
    private bool _disposed;








    public OnnxChatClient(
        string modelDirectory,
        ILogger<OnnxChatClient>? logger = null,
        string? systemPrompt = null,
        int maxLength = 2048,
        int maxNewTokens = 512)
    {
        if (string.IsNullOrWhiteSpace(modelDirectory))
            throw new ArgumentException("Model directory is required.", nameof(modelDirectory));

        _modelDirectory = modelDirectory;
        _logger = logger;
        _systemPrompt = string.IsNullOrWhiteSpace(systemPrompt) ? "You are a helpful assistant." : systemPrompt;

        if (maxLength <= 0) throw new ArgumentOutOfRangeException(nameof(maxLength));
        if (maxNewTokens <= 0) throw new ArgumentOutOfRangeException(nameof(maxNewTokens));
        _maxLength = maxLength;
        _maxNewTokens = maxNewTokens;

        // Basic IServiceProvider behavior.
        _services.TryAdd((typeof(IChatClient), null), this);
    }








    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Tokenizer/model are unmanaged wrappers.
        _tokenizer?.Dispose();
        _model?.Dispose();

        _tokenizer = null;
        _model = null;
    }








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
        {
            if (!string.IsNullOrEmpty(update.Text)) sb.Append(update.Text);
        }

        // Keep result shape minimal and compatible. Some preview versions include different members.
        // The common constructor takes a ChatMessage.
        var assistant = new ChatMessage(ChatRole.Assistant, sb.ToString());
        return new ChatResponse(assistant);
    }








    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messages);
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();

        EnsureModelLoaded();
        var model = _model!;
        var tokenizer = _tokenizer!;

        string prompt = BuildPrompt(messages, _systemPrompt);

        var generatorParams = new GeneratorParams(model);
        using var inputSequences = tokenizer.Encode(prompt);

        // max_length is the hard cap for total sequence length, including prompt.
        generatorParams.SetSearchOption("max_length", _maxLength);

        // Many models also respect max_new_tokens; if unsupported, it will be ignored.
        generatorParams.SetSearchOption("max_new_tokens", _maxNewTokens);

        SetInputSequences(generatorParams, inputSequences);

        // Capture is optional; ignore failures.
        try
        {
            generatorParams.TryGraphCaptureWithMaxBatchSize(1);
        }
        catch (Exception ex)
        {
            _logger?.LogDebug(ex, "ONNX GenAI graph capture was not available.");
        }

        using var tokenizerStream = tokenizer.CreateStream();
        using var generator = new Generator(model, generatorParams);

        StringBuilder fullText = new();
        while (!generator.IsDone())
        {
            cancellationToken.ThrowIfCancellationRequested();

            string? part;
            try
            {
                GenerateNextToken(generator);
                part = tokenizerStream.Decode(generator.GetSequence(0)[^1]);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "ONNX GenAI inference failed.");
                throw new InvalidOperationException("ONNX GenAI inference failed.", ex);
            }

            if (string.IsNullOrEmpty(part))
            {
                await Task.Yield();
                continue;
            }

            fullText.Append(part);
            if (ContainsStopToken(fullText)) yield break;

            yield return CreateUpdate(part);

            // Let the UI breathe in WPF.
            await Task.Yield();
        }
    }








    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ThrowIfDisposed();

        _services.TryGetValue((serviceType, serviceKey), out var value);
        return value;
    }








    public void SetService(Type serviceType, object? serviceKey, object? instance)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ThrowIfDisposed();

        _services[(serviceType, serviceKey)] = instance;
    }








    private void EnsureModelLoaded()
    {
        if (_model is not null && _tokenizer is not null) return;

        lock (_gate)
        {
            if (_model is not null && _tokenizer is not null) return;

            try
            {
                _model = new Model(_modelDirectory);
                _tokenizer = new Tokenizer(_model);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to load ONNX GenAI model from '{ModelDirectory}'.", _modelDirectory);
                throw new InvalidOperationException(
                    $"Failed to load ONNX GenAI model from '{_modelDirectory}'. Ensure the model files are present.",
                    ex);
            }
        }
    }








    private static string BuildPrompt(IEnumerable<ChatMessage> messages, string defaultSystemPrompt)
    {
        // Default to Phi-3 style prompt format.
        // If the app passes explicit system role messages, preserve them.
        const string systemTag = "<|system|>";
        const string userTag = "<|user|>";
        const string assistantTag = "<|assistant|>";
        const string endTag = "<|end|>";

        StringBuilder sb = new();

        bool hasSystem = false;
        foreach (ChatMessage m in messages)
        {
            if (m.Role == ChatRole.System) { hasSystem = true; break; }
        }

        if (!hasSystem && !string.IsNullOrWhiteSpace(defaultSystemPrompt))
        {
            sb.Append(systemTag).Append(defaultSystemPrompt).Append(endTag);
        }

        foreach (ChatMessage m in messages)
        {
            string text = m.Text ?? string.Empty;
            if (m.Role == ChatRole.System)
            {
                sb.Append(systemTag).Append(text).Append(endTag);
            }
            else if (m.Role == ChatRole.User)
            {
                sb.Append(userTag).Append(text).Append(endTag);
            }
            else if (m.Role == ChatRole.Assistant)
            {
                sb.Append(assistantTag).Append(text).Append(endTag);
            }
            else
            {
                // Unknown role: treat as user.
                sb.Append(userTag).Append(text).Append(endTag);
            }
        }

        sb.Append(assistantTag);
        return sb.ToString();
    }

    private static bool ContainsStopToken(StringBuilder sb)
    {
        // Match common Phi stop tags. We check the accumulated text to avoid splitting tokens.
        var s = sb.ToString();
        return s.Contains("<|end|>", StringComparison.Ordinal)
               || s.Contains("<|user|>", StringComparison.Ordinal)
               || s.Contains("<|system|>", StringComparison.Ordinal);
    }

    private static ChatResponseUpdate CreateUpdate(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        // ChatResponseUpdate API has shifted; keep it version tolerant.
        // Prefer a parameterless ctor with a settable Text property.
        var update = (ChatResponseUpdate?)Activator.CreateInstance(typeof(ChatResponseUpdate));
        if (update is null) throw new InvalidOperationException("Unable to create ChatResponseUpdate instance.");

        var p = update.GetType().GetProperty("Text");
        if (p is not null && p.CanWrite) p.SetValue(update, text);
        return update;
    }

    private static void SetInputSequences(GeneratorParams generatorParams, object sequences)
    {
        ArgumentNullException.ThrowIfNull(generatorParams);
        ArgumentNullException.ThrowIfNull(sequences);

        // Package versions vary: SetInputSequences, SetInputIds, SetInput, etc.
        var t = generatorParams.GetType();
        var mi = t.GetMethod("SetInputSequences")
                 ?? t.GetMethod("SetInput")
                 ?? t.GetMethod("SetInputIds")
                 ?? t.GetMethod("SetInputTokens");

        if (mi is null)
            throw new MissingMethodException(t.FullName, "SetInputSequences/SetInput/SetInputIds/SetInputTokens");

        mi.Invoke(generatorParams, [sequences]);
    }

    private static void GenerateNextToken(Generator generator)
    {
        ArgumentNullException.ThrowIfNull(generator);

        // Some versions require ComputeLogits() then GenerateNextToken(), others just GenerateNextToken().
        var t = generator.GetType();

        var compute = t.GetMethod("ComputeLogits");
        compute?.Invoke(generator, null);

        var next = t.GetMethod("GenerateNextToken")
                   ?? t.GetMethod("GenerateNext")
                   ?? t.GetMethod("NextToken");
        if (next is null)
            throw new MissingMethodException(t.FullName, "GenerateNextToken/GenerateNext/NextToken");

        next.Invoke(generator, null);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(OnnxChatClient));
    }
}