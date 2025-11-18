using System.Linq;
using System.Linq.Dynamic.Core;
using AquaPP.Models;
using AquaPP.ViewModels.Pages;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Serilog;
using Serilog.Enrichers.CallerInfo;
using Serilog.Enrichers.CallStack;
using ILogger = Serilog.ILogger;

namespace AquaPP.Views.Pages;

public partial class DataEntryView : UserControl
{
    
    private readonly ILogger _logger = new LoggerConfiguration()
        .Enrich.WithCallerInfo(includeFileInfo:true, allowedAssemblies:  ["AquaPP", "SukiUI", "Avalonia"])
        .WriteTo.Console()
        .WriteTo.File("app-debug.log")
        .Enrich.FromLogContext()
        .Enrich.WithCallStack()
        .CreateLogger();
    
    public DataEntryView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is DataEntryViewModel viewModel)
        {
            viewModel.RequestScrollToBottom += ViewModel_RequestScrollToBottom;
        }
    }
    
    private void ViewModel_RequestScrollToBottom()
    {
        var items = ReadingsDataGrid.ItemsSource.ToDynamicList(); 
        if (items.Count > 0)
        {
            ReadingsDataGrid.ScrollIntoView(items[^1], null);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (DataContext is DataEntryViewModel viewModel)
        {
            viewModel.RequestScrollToBottom -= ViewModel_RequestScrollToBottom;
        }
    }
    
    private void OnDataGridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not DataEntryViewModel viewModel) return;
        
        _logger.Information("OnDataGridSelectionChanged event fired successfully");
        viewModel.SelectedReadings.Clear();
        foreach (var item in ReadingsDataGrid.SelectedItems)
        {
            if (item is WaterQualityReading reading)
            {
                viewModel.SelectedReadings.Add(reading);
            }
        }
    }

    private void DataGridPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // We only care about left-clicks
        if (!e.GetCurrentPoint(sender as Visual).Properties.IsLeftButtonPressed)
        {
            return;
        }

        // Find the cell that was clicked.
        // We get the original source of the event and walk up the visual tree to find the DataGridCell.
        var cell = (e.Source as Visual)?.GetSelfAndVisualAncestors()
            .FirstOrDefault();

        if (cell == null)
        {
            return;
        }

        // Get the DataGrid itself from the 'sender' argument
        if (sender is not DataGrid grid)
        {
            return;
        }

        // Prevent editing if the grid is read-only or the cell is already being edited
        if (grid.IsReadOnly)
        {
            return;
        }

        // Find the parent row
        var row = cell.GetVisualParent<DataGridRow>();
        if (row == null)
        {
            return;
        }

        // Set the grid's current item and column to the one that was clicked
        grid.SelectedItem = row.DataContext;

        // Begin the edit
        if (grid.BeginEdit())
        {
            e.Handled = true;
        }
    }
}
