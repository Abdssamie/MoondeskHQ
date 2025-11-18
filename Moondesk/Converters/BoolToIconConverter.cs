using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AquaPP.Converters;

public class BoolToIconConverter : IValueConverter
{
    public static readonly BoolToIconConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool hasChanges)
        {
            return hasChanges ? "fa-solid fa-pen" : "fa-solid fa-check";
        }

        return "fa-solid fa-check";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
