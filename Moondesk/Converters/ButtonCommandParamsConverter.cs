using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace AquaPP.Converters;

public class ButtonCommandParamsConverter : IMultiValueConverter
{

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        // The values are already a list-like collection, we just need to return them
        // as an array for your C# code to cast successfully.
        return values.ToArray();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}