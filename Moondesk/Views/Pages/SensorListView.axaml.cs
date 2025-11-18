using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AquaPP.ViewModels.Pages;

namespace AquaPP.Views.Pages;

public partial class SensorListView : UserControl
{
    public SensorListView()
    {
        InitializeComponent();
        
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SensorListViewModel viewModel)
        {
            await viewModel.LoadDataCommand.ExecuteAsync(null);
        }
        
        // Find the DataGrid and attach double-click handler
        var dataGrid = this.FindControl<DataGrid>("SensorDataGrid");
        if (dataGrid != null)
        {
            dataGrid.DoubleTapped += OnDataGridDoubleTapped;
        }
    }

    private void OnDataGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is DataGrid dataGrid && 
            dataGrid.SelectedItem is SensorListItemModel selectedSensor &&
            DataContext is SensorListViewModel viewModel)
        {
            viewModel.NavigateToDetailCommand.Execute(selectedSensor.SensorId);
        }
    }
}
