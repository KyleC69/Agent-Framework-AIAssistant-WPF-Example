using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using AgentOrchestration.Wpf.Utils;

using Microsoft.Extensions.AI;




namespace AgentOrchestration.Wpf.Models;





// Represents the chat history between the user and the AI.
// Convenience collection for missing implementation in Agent Framework integration
public class ChatHistory : IList<ChatMessage>, IReadOnlyList<ChatMessage>, INotifyPropertyChanged, INotifyCollectionChanged
{
    /// <summary>The messages.</summary>
    private readonly List<ChatMessage> _messages;

    private Action<ChatMessage>? _overrideAdd;
    private Action<IEnumerable<ChatMessage>>? _overrideAddRange;
    private Action? _overrideClear;
    private Action<int, ChatMessage>? _overrideInsert;
    private Func<ChatMessage, bool>? _overrideRemove;
    private Action<int>? _overrideRemoveAt;
    private Action<int, int>? _overrideRemoveRange;








    /// <summary>Initializes an empty history.</summary>
    /// <summary>
    ///     Creates a new instance of the <see cref="ChatHistory" /> class
    /// </summary>
    public ChatHistory()
    {
        _messages = [];
    }








    /// <summary>
    ///     Creates a new instance of the <see cref="ChatHistory" /> with a first message in the provided
    ///     <see cref="ChatRole" />.
    ///     If not role is provided then the first message will default to <see cref="ChatRole.System" /> role.
    /// </summary>
    /// <param name="message">The text message to add to the first message in chat history.</param>
    /// <param name="role">The role to add as the first message.</param>
    public ChatHistory(string message, ChatRole role)
    {
        Verify.NotNullOrWhiteSpace(message);

        _messages = [new ChatMessage(role, message)];
    }








    /// <summary>
    ///     Creates a new instance of the <see cref="ChatHistory" /> class with a system message.
    /// </summary>
    /// <param name="systemMessage">The system message to add to the history.</param>
    public ChatHistory(string systemMessage)
            : this(systemMessage, ChatRole.System)
    {
    }








    /// <summary>Initializes the history will all of the specified messages.</summary>
    /// <param name="messages">The messages to copy into the history.</param>
    /// <exception cref="ArgumentNullException"><paramref name="messages" /> is null.</exception>
    public ChatHistory(IEnumerable<ChatMessage> messages)
    {
        Verify.NotNull(messages);
        _messages = [.. messages];
    }








    /// <summary>Gets or sets the message at the specified index in the history.</summary>
    /// <param name="index">The index of the message to get or set.</param>
    /// <returns>The message at the specified index.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index" /> was not valid for this history.</exception>
    public virtual ChatMessage this[int index]
    {
        get => _messages[index];
        set
        {
            Verify.NotNull(value);
            ChatMessage old = _messages[index];
            _messages[index] = value;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace, value, old, index));
        }
    }





    /// <inheritdoc />
    bool ICollection<ChatMessage>.IsReadOnly => false;





    /// <summary>Gets the number of messages in the history.</summary>
    public virtual int Count => _messages.Count;








    /// <summary>Removes all messages from the history.</summary>
    public virtual void Clear()
    {
        if (_messages.Count == 0)
        {
            _overrideClear?.Invoke();
            return;
        }

        _messages.Clear();
        _overrideClear?.Invoke();

        OnPropertyChanged(nameof(Count));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }








    /// <summary>Removes the message at the specified index from the history.</summary>
    /// <param name="index">The index of the message to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index" /> was not valid for this history.</exception>
    public virtual void RemoveAt(int index)
    {
        ChatMessage old = _messages[index];
        _messages.RemoveAt(index);
        _overrideRemoveAt?.Invoke(index);

        OnPropertyChanged(nameof(Count));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove, old, index));
    }








    /// <inheritdoc />
    IEnumerator<ChatMessage> IEnumerable<ChatMessage>.GetEnumerator()
    {
        return _messages.GetEnumerator();
    }








    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _messages.GetEnumerator();
    }








    /// <summary>Adds a message to the history.</summary>
    /// <param name="item">The message to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="item" /> is null.</exception>
    public virtual void Add(ChatMessage item)
    {
        Verify.NotNull(item);
        _messages.Add(item);
        _overrideAdd?.Invoke(item);

        OnPropertyChanged(nameof(Count));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, item, _messages.Count - 1));
    }








    /// <summary>Inserts a message into the history at the specified index.</summary>
    /// <param name="index">The index at which the item should be inserted.</param>
    /// <param name="item">The message to insert.</param>
    /// <exception cref="ArgumentNullException"><paramref name="item" /> is null.</exception>
    public virtual void Insert(int index, ChatMessage item)
    {
        Verify.NotNull(item);
        _messages.Insert(index, item);
        _overrideInsert?.Invoke(index, item);

        OnPropertyChanged(nameof(Count));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, item, index));
    }








    /// <summary>
    ///     Copies all of the messages in the history to an array, starting at the specified destination array index.
    /// </summary>
    /// <param name="array">The destination array into which the messages should be copied.</param>
    /// <param name="arrayIndex">The zero-based index into <paramref name="array" /> at which copying should begin.</param>
    /// <exception cref="ArgumentNullException"><paramref name="array" /> is null.</exception>
    /// <exception cref="ArgumentException">
    ///     The number of messages in the history is greater than the available space from
    ///     <paramref name="arrayIndex" /> to the end of <paramref name="array" />.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex" /> is less than 0.</exception>
    public virtual void CopyTo(ChatMessage[] array, int arrayIndex)
    {
        _messages.CopyTo(array, arrayIndex);
    }








    /// <summary>Determines whether a message is in the history.</summary>
    /// <param name="item">The message to locate.</param>
    /// <returns>true if the message is found in the history; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="item" /> is null.</exception>
    public virtual bool Contains(ChatMessage item)
    {
        Verify.NotNull(item);
        return _messages.Contains(item);
    }








    /// <summary>Searches for the specified message and returns the index of the first occurrence.</summary>
    /// <param name="item">The message to locate.</param>
    /// <returns>The index of the first found occurrence of the specified message; -1 if the message could not be found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="item" /> is null.</exception>
    public virtual int IndexOf(ChatMessage item)
    {
        Verify.NotNull(item);
        return _messages.IndexOf(item);
    }








    /// <summary>Removes the first occurrence of the specified message from the history.</summary>
    /// <param name="item">The message to remove from the history.</param>
    /// <returns>true if the item was successfully removed; false if it wasn't located in the history.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="item" /> is null.</exception>
    public virtual bool Remove(ChatMessage item)
    {
        Verify.NotNull(item);
        int index = _messages.IndexOf(item);
        if (index < 0)
        {
            return false;
        }

        bool result = _messages.Remove(item);
        _ = _overrideRemove?.Invoke(item);

        if (result)
        {
            OnPropertyChanged(nameof(Count));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove, item, index));
        }

        return result;
    }








    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public event PropertyChangedEventHandler? PropertyChanged;








    private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }








    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }








    // This allows observation of the chat history changes by-reference  reflecting in an
    // internal IEnumerable<Microsoft.Extensions.AI.ChatMessage> when used from IChatClients
    // with AutoFunctionInvocationFilters
    internal void SetOverrides(
            Action<ChatMessage> overrideAdd,
            Func<ChatMessage, bool> overrideRemove,
            Action onClear,
            Action<int, ChatMessage> overrideInsert,
            Action<int> overrideRemoveAt,
            Action<int, int> overrideRemoveRange,
            Action<IEnumerable<ChatMessage>> overrideAddRange)
    {
        _overrideAdd = overrideAdd;
        _overrideRemove = overrideRemove;
        _overrideClear = onClear;
        _overrideInsert = overrideInsert;
        _overrideRemoveAt = overrideRemoveAt;
        _overrideRemoveRange = overrideRemoveRange;
        _overrideAddRange = overrideAddRange;
    }








    internal void ClearOverrides()
    {
        _overrideAdd = null;
        _overrideRemove = null;
        _overrideClear = null;
        _overrideInsert = null;
        _overrideRemoveAt = null;
        _overrideRemoveRange = null;
        _overrideAddRange = null;
    }








    /// <summary>
    ///     <param name="ChatRole">Role of the message author</param>
    ///     <param name="content">Message content</param>
    ///     <param name="encoding">Encoding of the message content</param>
    ///     <param name="metadata">Dictionary for any additional metadata</param>
    /// </summary>
    public void AddMessage(ChatRole ChatRole, string content)
    {
        Add(new ChatMessage(ChatRole, content));
    }








    /// <summary>
    ///     Add a user message to the chat history
    /// </summary>
    /// <param name="content">Message content</param>
    public void AddUserMessage(string content)
    {
        AddMessage(ChatRole.User, content);
    }








    /// <summary>
    ///     Add an assistant message to the chat history
    /// </summary>
    /// <param name="content">Message content</param>
    public void AddAssistantMessage(string content)
    {
        AddMessage(ChatRole.Assistant, content);
    }








    /// <summary>
    ///     Add a system message to the chat history
    /// </summary>
    /// <param name="content">Message content</param>
    public void AddSystemMessage(string content)
    {
        AddMessage(ChatRole.System, content);
    }








    /// <summary>
    ///     Add a tool message to the chat history
    /// </summary>
    /// <param name="content">Message content</param>
    public void AddToolMessage(string content)
    {
        AddMessage(ChatRole.Tool, content);
    }








    /// <summary>Adds the messages to the history.</summary>
    /// <param name="items">The collection whose messages should be added to the history.</param>
    /// <exception cref="ArgumentNullException"><paramref name="items" /> is null.</exception>
    public virtual void AddRange(IEnumerable<ChatMessage> items)
    {
        Verify.NotNull(items);
        var added = items as IList<ChatMessage> ?? items.ToList();
        if (added.Count == 0)
        {
            return;
        }

        int startIndex = _messages.Count;

        _messages.AddRange(added);
        _overrideAddRange?.Invoke(added);

        OnPropertyChanged(nameof(Count));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, added, startIndex));
    }








    /// <summary>
    ///     Removes a range of messages from the history.
    /// </summary>
    /// <param name="index">The index of the range of elements to remove.</param>
    /// <param name="count">The number of elements to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is less than 0.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count" /> is less than 0.</exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="count" /> and <paramref name="count" /> do not denote a valid range
    ///     of messages.
    /// </exception>
    public virtual void RemoveRange(int index, int count)
    {
        if (count <= 0)
        {
            return;
        }

        var removed = _messages.GetRange(index, count);
        _messages.RemoveRange(index, count);
        _overrideRemoveRange?.Invoke(index, count);

        OnPropertyChanged(nameof(Count));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove, removed, index));
    }
}