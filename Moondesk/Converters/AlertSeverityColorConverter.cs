using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using AquaPP.Core.Models.IoT;

namespace AquaPP.Converters;

public class AlertSeverityColorConverter : IValueConverter
{
    public static readonly AlertSeverityColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is AlertSeverity severity)
        {
            var colorHex = severity switch
            {
                AlertSeverity.Info => "#2196F3",      // Blue
                AlertSeverity.Warning => "#FF9800",   // Orange
                AlertSeverity.Critical => "#F44336",  // Red
                AlertSeverity.Emergency => "#9C27B0", // Purple
                _ => "#9E9E9E"                        // Gray
            };

            return SolidColorBrush.Parse(colorHex);
        }

        return new SolidColorBrush(Colors.Gray);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
