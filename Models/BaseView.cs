namespace printFlowTui.Models;

using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;

public class BaseView : View
{
    public BaseView()
    {
        BorderStyle = LineStyle.Rounded;
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;
}
