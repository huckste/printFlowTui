namespace printFlowTui.Views;

using System.Collections.ObjectModel;
using printFlowTui.Models;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

public class PrinterQueuePanel : View
{
    public ListView QueueList { get; }
    public string PrinterName { get; }
    public Label LabelCount { get; }
    public ObservableCollection<FileToPrint> Queue { get; }

    public PrinterQueuePanel(string printerName)
    {
        PrinterName = printerName;
        Queue = [];

        Title = printerName;
        X = 0;
        Y = 0;
        Width = Dim.Percent(50);
        Height = Dim.Auto();
        BorderStyle = LineStyle.Rounded;

        QueueList = new ListView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Auto(),
            BorderStyle = LineStyle.None,
        };

        LabelCount = new Label { X = 1, Y = Pos.Bottom(QueueList) + 1 };

        QueueList.SetSource(Queue);

        Add(QueueList, LabelCount);
    }

    public void UpdateLabelCount(int count) => LabelCount.Text = $"LabelCount: {count}";
}
