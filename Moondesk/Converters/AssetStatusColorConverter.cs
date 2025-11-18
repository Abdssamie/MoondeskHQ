using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using AquaPP.Core.Models.IoT;

namespace AquaPP.Converters;

public class AssetStatusColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is AssetStatus status)
        {
            var colorHex = status switch
            {
                AssetStatus.Online => "#4CAF50",      // Green
                AssetStatus.Offline => "#9E9E9E",     // Gray
                AssetStatus.Warning => "#FF9800",     // Orange
                AssetStatus.Critical => "#F44336",    // Red
                AssetStatus.Maintenance => "#2196F3", // Blue
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
