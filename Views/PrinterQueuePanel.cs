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
        Width = Dim.Percent(80);
        Height = Dim.Auto();
        BorderStyle = LineStyle.Rounded;
        CanFocus = true;
        TabStop = TabBehavior.TabGroup;

        QueueList = new ListView
        {
            Width = Dim.Fill(),
            Height = Dim.Auto(),
            BorderStyle = LineStyle.None,
            TabStop = TabBehavior.TabStop,
            MarkMultiple = true,
            ShowMarks = true,
            CanFocus = true,
        };

        LabelCount = new Label { X = Pos.Center(), Y = Pos.Center() };

        HasFocusChanged += (sender, e) =>
        {
            if (e.CurrentValue)
            {
                BorderStyle = LineStyle.Double;
                Remove(LabelCount);
                Add(QueueList);
            }
            else
            {
                BorderStyle = LineStyle.Rounded;
                Add(LabelCount);
                Remove(QueueList);
            }
        };

        QueueList.SetSource(Queue);

        Add(LabelCount);
    }

    public void UpdateLabelCount() =>
        LabelCount.Text = $"{Queue.Count} Files | {Queue.Sum(f => f.LabelCount)} Labels";
}
