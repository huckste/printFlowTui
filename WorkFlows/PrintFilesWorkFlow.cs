using System.Collections.ObjectModel;
using printFlowTui.Models;
using printFlowTui.Services;
using printFlowTui.Views;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace printFlowTui.WorkFlows;

public class PrintFilesWorkFlow : View
{
    public event EventHandler? Finished;

    // === Views this workflow manages ===
    private readonly Dialog _operationsModal;
    private readonly OptionSelector _operationSelector;
    private readonly OptionSelector _printerSelector;
    private readonly FileSelectionView _fileSelectionView;
    private readonly PrinterQueueView _printerQueueView;

    // === Data this workflow needs ===
    private readonly string _printerPath;
    private readonly List<Printer> _printers = [];
    private IEnumerable<int>? _markedIndices;

    public PrintFilesWorkFlow()
    {
        Width = Dim.Percent(90);
        Height = Dim.Percent(90);
        X = Pos.Center();
        Y = Pos.Center();
        CanFocus = true;

        _fileSelectionView = new FileSelectionView { };
        _printerQueueView = new PrinterQueueView { X = Pos.Right(_fileSelectionView) };

        _printerPath = "//home/huckste/TestFolder/Printers/";

        _operationsModal = new Dialog
        {
            Title = "File Operation",
            BorderStyle = LineStyle.Rounded,
            ShadowStyle = ShadowStyle.None,
            CanFocus = true,
        };

        _operationSelector = new OptionSelector
        {
            X = 1,
            Y = 1,
            Labels = ["Select Printer", "Duplicate", "Split", "Delete"],
        };

        _printerSelector = new OptionSelector { X = 1, Y = 1 };

        // === Events this workflow handles ==
        _fileSelectionView.FilesSelected += OnFilesSelected;
        _operationSelector.Accepting += OnOperationSelected;
        _printerSelector.Accepting += OnPrinterSelected;

        Add(_fileSelectionView, _printerQueueView);
    }

    // === Initialization ===
    public async Task InitalizeAsync()
    {
        await _fileSelectionView.InitializeAsync();

        var printers = await FetchFiles.Printers(_printerPath);

        foreach (var printer in printers)
        {
            _printers.Add(printer);
        }

        _printerSelector.Labels = _printers.Select(p => p.PrinterName).ToArray();
    }

    // === Event handlers — the workflow logic ===
    private void OnFilesSelected(object? sender, FileSelectionView.FilesSelectedEventArgs e)
    {
        _markedIndices = e.MarkedIndices;

        if (!_markedIndices.Any())
            return;

        _operationsModal.Add(_operationSelector);
        Remove(_printerQueueView);
        Remove(_fileSelectionView);
        Add(_operationsModal);
    }

    private void OnOperationSelected(object? sender, CommandEventArgs e)
    {
        _operationsModal.Remove(_operationSelector);

        switch (_operationSelector.Value)
        {
            case 0:
                _operationsModal.Add(_printerSelector);
                break;
            default:
                CloseModal();
                break;
        }

        e.Handled = true;
    }

    private void OnPrinterSelected(object? sender, CommandEventArgs e)
    {
        var idx = _printerSelector.Value ?? 0;
        var files = _fileSelectionView.RemoveMarkedFiles(_markedIndices);
        _markedIndices = null;

        if (files != null)
        {
            var queue = new ObservableCollection<FileToPrint>(files);
            _printerQueueView.AddOrUpdatePrinterPanel(_printers[idx].PrinterName, queue);
        }

        _fileSelectionView.UpdateStatus();

        CloseModal();

        e.Handled = true;
    }

    private void CloseModal()
    {
        _operationsModal.RemoveAll();
        Remove(_operationsModal);
        Add(_fileSelectionView);
        Add(_printerQueueView);
    }
}
