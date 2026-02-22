using System;
using System.Globalization;
using System.Windows.Data;




namespace AgentOrchestration.Wpf.Controls;





public sealed class BubbleMaxWidthConverter : IValueConverter
{
    private const double HorizontalPadding = 64;
    private const double MaxBubbleWidth = 980;








    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double width || double.IsNaN(width) || double.IsInfinity(width))
        {
            return MaxBubbleWidth;
        }

        var max = Math.Max(0, width - HorizontalPadding);
        return Math.Min(MaxBubbleWidth, max);
    }








    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}