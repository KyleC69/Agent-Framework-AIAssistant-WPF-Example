using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;



namespace AgentOrch.ChatApp.Wpf.Controls;


/// <summary>
///     Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
///     Step 1a) Using this custom control in a XAML file that exists in the current project.
///     Add this XmlNamespace attribute to the root element of the markup file where it is
///     to be used:
///     xmlns:MyNamespace="clr-namespace:AgentOrch.ChatApp.Wpf.Controls"
///     Step 1b) Using this custom control in a XAML file that exists in a different project.
///     Add this XmlNamespace attribute to the root element of the markup file where it is
///     to be used:
///     xmlns:MyNamespace="clr-namespace:AgentOrch.ChatApp.Wpf.Controls;assembly=AgentOrch.ChatApp.Wpf.Controls"
///     You will also need to add a project reference from the project where the XAML file lives
///     to this project and Rebuild to avoid compilation errors:
///     Right click on the target project in the Solution Explorer and
///     "Add Reference"->"Projects"->[Browse to and select this project]
///     Step 2)
///     Go ahead and use your control in the XAML file.
///     <MyNamespace:AIToolUsageIndicators />
/// </summary>
public class AIToolUsageIndicators : Control
{
    public const int DefaultIndicatorCount = 5;

    public static readonly DependencyProperty IndicatorCountProperty =
        DependencyProperty.Register(
            nameof(IndicatorCount),
            typeof(int),
            typeof(AIToolUsageIndicators),
            new FrameworkPropertyMetadata(DefaultIndicatorCount, OnIndicatorCountChanged),
            ValidateIndicatorCount);

    public static readonly DependencyProperty IndicatorSizeProperty =
        DependencyProperty.Register(
            nameof(IndicatorSize),
            typeof(double),
            typeof(AIToolUsageIndicators),
            new FrameworkPropertyMetadata(100d));

    public static readonly DependencyProperty OffBrushProperty =
        DependencyProperty.Register(
            nameof(OffBrush),
            typeof(Brush),
            typeof(AIToolUsageIndicators),
            new FrameworkPropertyMetadata(Brushes.Red));

    public static readonly DependencyProperty OnBrushProperty =
        DependencyProperty.Register(
            nameof(OnBrush),
            typeof(Brush),
            typeof(AIToolUsageIndicators),
            new FrameworkPropertyMetadata(Brushes.LimeGreen));

    public static readonly DependencyProperty HoldDurationProperty =
        DependencyProperty.Register(
            nameof(HoldDuration),
            typeof(TimeSpan),
            typeof(AIToolUsageIndicators),
            new FrameworkPropertyMetadata(TimeSpan.FromSeconds(3), null, CoerceHoldDuration));








    static AIToolUsageIndicators()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AIToolUsageIndicators), new FrameworkPropertyMetadata(typeof(AIToolUsageIndicators)));
    }








    public AIToolUsageIndicators()
    {
        Indicators = [];
        EnsureIndicatorCount();
    }








    public int IndicatorCount
    {
        get => (int)GetValue(IndicatorCountProperty);
        set => SetValue(IndicatorCountProperty, value);
    }





    public double IndicatorSize
    {
        get => (double)GetValue(IndicatorSizeProperty);
        set => SetValue(IndicatorSizeProperty, value);
    }





    public Brush OffBrush
    {
        get => (Brush)GetValue(OffBrushProperty);
        set => SetValue(OffBrushProperty, value);
    }





    public Brush OnBrush
    {
        get => (Brush)GetValue(OnBrushProperty);
        set => SetValue(OnBrushProperty, value);
    }





    public TimeSpan HoldDuration
    {
        get => (TimeSpan)GetValue(HoldDurationProperty);
        set => SetValue(HoldDurationProperty, value);
    }





    public ObservableCollection<IndicatorState> Indicators { get; }








    /// <summary>
    ///     Triggers the indicator at <paramref name="index" /> to turn on and remain on for at least
    ///     <see cref="HoldDuration" />,
    ///     then returns to the off state.
    /// </summary>
    public void TriggerIndicator(int index)
    {
        if (index < 0 || index >= Indicators.Count) throw new ArgumentOutOfRangeException(nameof(index));

        Indicators[index].Pulse(HoldDuration);
    }








    private static bool ValidateIndicatorCount(object value)
    {
        return value is int count && count > 0;
    }








    private static void OnIndicatorCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AIToolUsageIndicators control) control.EnsureIndicatorCount();
    }








    private void EnsureIndicatorCount()
    {
        var target = IndicatorCount;
        if (target < 1) target = 1;

        while (Indicators.Count < target) Indicators.Add(new IndicatorState());

        while (Indicators.Count > target) Indicators.RemoveAt(Indicators.Count - 1);
    }








    private static object CoerceHoldDuration(DependencyObject d, object baseValue)
    {
        return baseValue is not TimeSpan ts ? TimeSpan.FromSeconds(3) : ts < TimeSpan.FromSeconds(3) ? TimeSpan.FromSeconds(3) : ts;
    }








    public sealed class IndicatorState : DependencyObject
    {
        public static readonly DependencyProperty IsOnProperty =
            DependencyProperty.Register(
                nameof(IsOn),
                typeof(bool),
                typeof(IndicatorState),
                new FrameworkPropertyMetadata(false));

        private CancellationTokenSource? _cts;





        public bool IsOn
        {
            get => (bool)GetValue(IsOnProperty);
            private set => SetValue(IsOnProperty, value);
        }








        public void Pulse(TimeSpan holdDuration)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            IsOn = true;

            _ = TurnOffLaterAsync(holdDuration, _cts.Token);
        }








        private async Task TurnOffLaterAsync(TimeSpan holdDuration, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(holdDuration, cancellationToken);
                if (!cancellationToken.IsCancellationRequested) IsOn = false;
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}