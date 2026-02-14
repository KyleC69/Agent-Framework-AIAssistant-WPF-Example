using System.Windows;
using System.Windows.Threading;

using AgentOrch.ChatApp.Wpf.Controls;
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

        ViewModel = App.GetService<MainViewModel>() ?? throw new InvalidOperationException("MainViewModel is not registered.");
        DataContext = ViewModel;


        //  Loaded += (_, _) => TestUILights();
    }








    public MainViewModel ViewModel { get; }








    private void TestUILights()
    {
        var y = 0;
        while (y < 200)
        {
            for (var x = 0; x < 6; x++)
            {
                AIToolIndicatorHub.Pulse(x);
                Thread.Sleep(200);
            }

            y++;
        }
    }








    private void ScrollToLatest()
    {
        if (MessagesList.Items.Count <= 0)
        {
            return;
        }

        var lastItem = MessagesList.Items[MessagesList.Items.Count - 1];
        MessagesList.ScrollIntoView(lastItem);
    }








    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        AIToolIndicatorHub.Register(ToolIndicators);
        _ = ViewModel.ConfigureAgentAsync();
    }








    private void ToggleButton_Checked(object sender, RoutedEventArgs e)
    {
        AIToolIndicatorHub.Pulse(0);
    }
}