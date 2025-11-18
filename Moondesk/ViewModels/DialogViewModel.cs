using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;

namespace AquaPP.ViewModels;

public partial class DialogViewModel(ISukiDialog dialog) : ObservableObject
{
    [RelayCommand]
    private void CloseDialog()
    {
        dialog.Dismiss();
    }
}
