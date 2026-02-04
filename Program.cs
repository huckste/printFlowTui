using Terminal.Gui.App;
using Terminal.Gui.FileServices;
using Terminal.Gui.Views;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Input;
using System.Reflection.Metadata;
using Terminal.Gui.Drawing;
using System.Diagnostics.Tracing;
using System.Collections.ObjectModel;

IApplication app = Application.Create().Init();

Window window = new();

var printFiles = new Shortcut(
    key: Key.F,
    commandText: "printFiles",
    action: () => { app.RequestStop(); }
    );



app.Run(window);
app.Dispose();

