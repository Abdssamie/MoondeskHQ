using System;
using SukiUI;

namespace AquaPP.Services;

public class ThemeProvider
{
    public ThemeProvider()
    {
        SukiTheme.GetInstance().OnBaseThemeChanged += variant => { Console.WriteLine("Theme changed triggered !"); };
    }
}