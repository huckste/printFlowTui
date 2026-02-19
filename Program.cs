using printFlowTui;
using Terminal.Gui.App;
using Terminal.Gui.Views;

using IApplication app = Application.Create().Init();
var window = new Window();
window.Add(new App());
app.Run(window);
window.Dispose();
