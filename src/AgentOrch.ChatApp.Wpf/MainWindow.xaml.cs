using System.Collections.Specialized;
using System.Windows;
using System.Windows.Threading;

using AgentOrch.ChatApp.Wpf.ViewModels;



namespace AgentOrch.ChatApp.Wpf;


/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private DispatcherOperation? _pendingScroll;








    public MainWindow()
    {
        InitializeComponent();

        ViewModel = App.GetService<MainViewModel>();


        ViewModel.Messages.CollectionChanged += OnMessagesCollectionChanged;
        Loaded += (_, _) => RequestScrollToLatest();
    }








    public MainViewModel? ViewModel { get; }








    private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RequestScrollToLatest();
    }








    private void RequestScrollToLatest()
    {
        if (!IsLoaded) return;

        _pendingScroll?.Abort();
        _pendingScroll = Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
        {
            _pendingScroll = null;
            ScrollToLatest();
        });
    }








    private void ScrollToLatest()
    {
        if (MessagesList.Items.Count <= 0) return;

        var lastItem = MessagesList.Items[MessagesList.Items.Count - 1];
        MessagesList.ScrollIntoView(lastItem);
    }








    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _ = ViewModel.ConfigureAgentAsync();
    }
}