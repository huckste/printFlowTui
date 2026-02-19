namespace printFlowTui.Services;

using printFlowTui.Views;
using Terminal.Gui.ViewBase;

public class PrinterPanelArrangement
{
    public static View? _lastRight = null;
    public static View? _lastLeft = null;

    public static void SetPanelPos(int col, PrinterQueuePanel panel)
    {
        if (col % 2 == 0) // Left column
        {
            panel.X = 1;
            panel.Y = _lastLeft != null ? Pos.Bottom(_lastLeft) : 0;
            panel.Width = Dim.Percent(49);
            _lastLeft = panel;
        }
        else // Right column
        {
            panel.X = Pos.Percent(51);
            panel.Y = _lastRight != null ? Pos.Bottom(_lastRight) : 0;
            panel.Width = Dim.Fill(1);
            _lastRight = panel;
        }
    }
}
