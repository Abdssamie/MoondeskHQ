using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AquaPP.Converters;

public class AlertStatusColorConverter : IValueConverter
{
    public static readonly AlertStatusColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool acknowledged)
        {
            // Green for acknowledged, Red for unacknowledged
            var colorHex = acknowledged ? "#4CAF50" : "#F44336";
            
            if (targetType == typeof(Color))
            {
                return Color.Parse(colorHex);
            }
            
            return SolidColorBrush.Parse(colorHex);
        }

        return new SolidColorBrush(Colors.Gray);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
