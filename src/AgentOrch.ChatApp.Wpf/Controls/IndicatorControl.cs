using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;




namespace AgentOrchestration.Wpf.Controls;





//Represents a single indicator light that can be used within any control.
//This control is designed to be simple and reusable, allowing it to be easily integrated
//into any control to represent the on/off state of something.
public class IndicatorControl : Control
{

    public static readonly DependencyProperty IndicatorSizeProperty =
            DependencyProperty.Register(
                    nameof(IndicatorSize),
                    typeof(double),
                    typeof(IndicatorControl),
                    new FrameworkPropertyMetadata(100d));

    public static readonly DependencyProperty OffBrushProperty =
            DependencyProperty.Register(
                    nameof(OffBrush),
                    typeof(Brush),
                    typeof(IndicatorControl),
                    new FrameworkPropertyMetadata(Brushes.Red));

    public static readonly DependencyProperty OnBrushProperty =
            DependencyProperty.Register(
                    nameof(OnBrush),
                    typeof(Brush),
                    typeof(IndicatorControl),
                    new FrameworkPropertyMetadata(Brushes.LimeGreen));



    public static readonly DependencyProperty IsModelReadyProperty =
            DependencyProperty.Register(
                    nameof(IsModelReady),
                    typeof(bool),
                    typeof(IndicatorControl),
                    new FrameworkPropertyMetadata(false));








    static IndicatorControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(IndicatorControl), new FrameworkPropertyMetadata(typeof(IndicatorControl)));
    }








    public bool IsModelReady
    {
        get { return (bool)this.GetValue(IsModelReadyProperty); }
        set { this.SetValue(IsModelReadyProperty, value); }
    }





    public double IndicatorSize
    {
        get { return (double)this.GetValue(IndicatorSizeProperty); }
        set { this.SetValue(IndicatorSizeProperty, value); }
    }





    public Brush OffBrush
    {
        get { return (Brush)this.GetValue(OffBrushProperty); }
        set { this.SetValue(OffBrushProperty, value); }
    }





    public Brush OnBrush
    {
        get { return (Brush)this.GetValue(OnBrushProperty); }
        set { this.SetValue(OnBrushProperty, value); }
    }





    public sealed class IndicatorState : DependencyObject
    {
        public static readonly DependencyProperty IsOnProperty =
                DependencyProperty.Register(
                        nameof(IsOn),
                        typeof(bool),
                        typeof(IndicatorState),
                        new FrameworkPropertyMetadata(false));





        public bool IsOn
        {
            get { return (bool)this.GetValue(IsOnProperty); }
            private set { this.SetValue(IsOnProperty, value); }
        }
    }
}