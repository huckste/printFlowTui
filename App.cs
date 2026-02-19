namespace printFlowTui;

using printFlowTui.WorkFlows;
using Terminal.Gui.ViewBase;

public class App : View
{
    private readonly PrintFilesWorkFlow _printFilesWorkflow;

    public App()
    {
        Width = Dim.Fill();
        Height = Dim.Fill();
        CanFocus = true;

        _printFilesWorkflow = new PrintFilesWorkFlow();

        Init();
    }

    public async void Init()
    {
        await _printFilesWorkflow.InitalizeAsync();
        Add(_printFilesWorkflow);
    }
}
