using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AquaPP.Converters;

public class BoolToStatusTextConverter : IValueConverter
{
    public static readonly BoolToStatusTextConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool hasChanges)
        {
            return hasChanges ? "Modified" : "Saved";
        }

        return "Saved";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
