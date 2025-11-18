using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace AquaPP.Converters;

public class ContentToControlConverter : IValueConverter
{
    public static readonly ContentToControlConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
            return new TextBlock() { Text = s };

        return value ?? throw new ArgumentNullException(nameof(value));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}