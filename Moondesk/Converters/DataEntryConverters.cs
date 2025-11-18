using System;
using System.Globalization;
using AquaPP.Models;
using Avalonia.Data.Converters;

namespace AquaPP.Converters;

public class EditModeToIconColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool editModeEnabled)
        {
            return editModeEnabled ? "Blue" : "Green";
        }
        return "Green";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}