using System.Windows.Threading;



namespace AgentOrch.ChatApp.Wpf.Controls;


internal static class AIToolIndicatorHub
{
    private static Dispatcher? _dispatcher;
    private static AIToolUsageIndicators? _control;








    public static void Register(AIToolUsageIndicators control)
    {
        ArgumentNullException.ThrowIfNull(control);

        _control = control;
        _dispatcher = control.Dispatcher;
    }








    public static void Pulse(int indicatorIndex)
    {
        AIToolUsageIndicators? control = _control;
        Dispatcher? dispatcher = _dispatcher;

        if (control is null || dispatcher is null) return;

        if (dispatcher.CheckAccess())
        {
            control.TriggerIndicator(indicatorIndex);
            return;
        }

        _ = dispatcher.BeginInvoke(() => control.TriggerIndicator(indicatorIndex));
    }
}