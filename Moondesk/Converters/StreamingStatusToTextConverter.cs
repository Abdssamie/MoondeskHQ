using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AquaPP.Converters;

public class StreamingStatusToTextConverter: IValueConverter
{
    public static readonly StreamingStatusToTextConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool hasChanges)
        {
            return hasChanges ? "Streaming" : "Start streaming";
        }

        return "Saved";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}