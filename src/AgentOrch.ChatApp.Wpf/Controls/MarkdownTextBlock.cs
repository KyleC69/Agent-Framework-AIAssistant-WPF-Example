using System.Windows;
using System.Windows.Controls;

using Markdig.Wpf;




namespace AgentOrchestration.Wpf.Controls;





public sealed class MarkdownTextBlock : UserControl
{
    public static readonly DependencyProperty MarkdownTextProperty =
            DependencyProperty.Register(
                    nameof(MarkdownText),
                    typeof(string),
                    typeof(MarkdownTextBlock),
                    new PropertyMetadata(string.Empty));








    public MarkdownTextBlock()
    {
        MarkdownViewer viewer = new()
        {
                Background = null,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Focusable = false,
                IsTabStop = false,
                VerticalAlignment = VerticalAlignment.Top
        };


        _ = viewer.SetBinding(MarkdownViewer.MarkdownProperty, new System.Windows.Data.Binding
        {
                Source = this,
                Path = new PropertyPath(nameof(MarkdownText))
        });

        Content = viewer;
    }








    public string MarkdownText
    {
        get { return (string)this.GetValue(MarkdownTextProperty); }
        set { this.SetValue(MarkdownTextProperty, value); }
    }
}