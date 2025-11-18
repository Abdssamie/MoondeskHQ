using System;
using System.Globalization;
using Avalonia.Data.Converters;
using AquaPP.Core.Models.IoT;

namespace AquaPP.Converters;

public class AssetStatusIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is AssetStatus status)
        {
            return status switch
            {
                AssetStatus.Online => "fa-solid fa-circle-check",
                AssetStatus.Offline => "fa-solid fa-circle-xmark",
                AssetStatus.Warning => "fa-solid fa-triangle-exclamation",
                AssetStatus.Critical => "fa-solid fa-circle-exclamation",
                AssetStatus.Maintenance => "fa-solid fa-wrench",
                _ => "fa-solid fa-circle-question"
            };
        }

        return "fa-solid fa-circle-question";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
