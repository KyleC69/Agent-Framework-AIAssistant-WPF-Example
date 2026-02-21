using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

using AgentOrchestration.Wpf.Controls;
using AgentOrchestration.Wpf.ViewModels;

using Microsoft.Extensions.AI;




namespace AgentOrchestration.Wpf;





/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly DispatcherOperation? _pendingScroll;








    public MainWindow()
    {
        InitializeComponent();

        ViewModel = App.GetService<MainViewModel>() ?? throw new InvalidOperationException("MainViewModel is not registered.");
        DataContext = ViewModel;


        _ = App.GetService<IChatClient>();
    }








    public MainViewModel ViewModel { get; }








    private void TestUILights()
    {
        int y = 0;
        while (y < 200)
        {
            for (int x = 0; x < 6; x++)
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

        object lastItem = MessagesList.Items[^1];
        MessagesList.ScrollIntoView(lastItem);
    }








    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        //  AIToolIndicatorHub.Register(ToolIndicators);
        _ = ViewModel.ConfigureAgentAsync();
    }








    private void ToggleButton_Checked(object sender, RoutedEventArgs e)
    {
        AIToolIndicatorHub.Pulse(0);
    }
}