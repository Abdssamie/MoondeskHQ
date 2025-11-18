using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AquaPP.Converters;

public class StreamingStatusToIconConverter: IValueConverter
{
    public static readonly StreamingStatusToIconConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool hasChanges)
        {
            return hasChanges ? "fa-solid fa-pause" : "fa-solid fa-play";
        }

        return "Saved";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}