using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.AI;




namespace AgentOrchestration.Wpf.ViewModels;





public sealed class ChatMessageViewModel : INotifyPropertyChanged
{
    private string _text;








    public ChatMessageViewModel(ChatRole role, string text, string? authorName = null)
    {
        Role = role;
        _text = text;
        AuthorName = authorName;
    }








    public ChatRole Role { get; }

    public string? AuthorName { get; }





    public string Text
    {
        get => _text;
        set
        {
            if (string.Equals(_text, value, StringComparison.Ordinal))
            {
                return;
            }

            _text = value;
            OnPropertyChanged();
        }
    }





    public event PropertyChangedEventHandler? PropertyChanged;








    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}