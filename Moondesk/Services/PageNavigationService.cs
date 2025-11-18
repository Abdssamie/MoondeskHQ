using System;
using AquaPP.ViewModels.Pages;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AquaPP.Services;

public class PageNavigationService
{
    public Action<Type>? NavigationRequested { get; set; }

    public void RequestNavigation<T>() where T : PageBase
    {
        NavigationRequested?.Invoke(typeof(T));
    }
}
