namespace printFlowTui.Views;

using System.Collections.ObjectModel;
using printFlowTui.Models;
using printFlowTui.Services;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

public class PrinterQueueView : BaseView
{
    public Label QueueStatus { get; }
    private readonly Dictionary<string, PrinterQueuePanel> _panels = [];

    public PrinterQueueView()
    {
        Title = "Printer Queue";
        X = 0;
        Y = 0;
        Width = Dim.Fill();
        Height = Dim.Auto();

        QueueStatus = new Label
        {
            Text = "Queue Empty",
            X = Pos.Center(),
            Y = Pos.Center(),
        };

        Add(QueueStatus);
    }

    public void AddOrUpdatePrinterPanel(
        string printerName,
        ObservableCollection<FileToPrint> newItems
    )
    {
        if (!_panels.TryGetValue(printerName, out var panel))
        {
            panel = new PrinterQueuePanel(printerName);
            _panels.Add(printerName, panel);
            PrinterPanelArrangement.SetPanelPos(_panels.Count - 1, panel);

            if (_panels.Count == 1)
                Remove(QueueStatus);

            Add(panel);
        }

        foreach (var file in newItems)
        {
            panel.Queue.Add(file);
        }

        panel.UpdateLabelCount(panel.Queue.Sum(f => f.LabelCount));
    }
}
