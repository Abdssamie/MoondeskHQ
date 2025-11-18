using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using AquaPP.Models;
using SukiUI;

namespace AquaPP.Converters;


public class SenderToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        SukiTheme theme = SukiTheme.GetInstance();
        
        var background =
            theme.ActiveBaseTheme.Equals(ThemeVariant.Dark) ? Brushes.DarkGray : Brushes.LightGray;
        
        return value is MessageSender ? background : Brushes.Transparent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class SenderToAlignmentConverter : IValueConverter
{
    // public static readonly SenderToBrushConverter Instance = new SenderToBrushConverter();
    
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is MessageSender sender)
        {
            return sender == MessageSender.User ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }
        return HorizontalAlignment.Left;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class SenderToIconData : IValueConverter
{
    public readonly String AgentIconData = "fa-solid fa-robot";
    public readonly String UserIconData = "fa-solid fa-circle-user";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is MessageSender sender)
        {
            return sender == MessageSender.User ? UserIconData: AgentIconData;
        }
        return UserIconData;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}