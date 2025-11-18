using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AquaPP.Converters;

public class BoolToStatusColorConverter : IValueConverter
{
    public static readonly BoolToStatusColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool hasChanges)
        {
            var colorHex = hasChanges ? "#FF9800" : "#4CAF50"; // Orange for changes, Green for saved
            return SolidColorBrush.Parse(colorHex);
        }

        return new SolidColorBrush(Colors.Gray);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
