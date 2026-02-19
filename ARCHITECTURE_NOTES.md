# PrintFlowTui Architecture Notes

## Issues Discovered & Solutions

### 1. IList<T> vs ObservableCollection<T> in ListView

**Problem:** Terminal.Gui's `ListView.SetSource<T>()` expects `ObservableCollection<T>`, not `IList<T>`. When you call `SetSource()`, the collection gets wrapped in an internal `IListDataSource`, so you can't retrieve the original `ObservableCollection` from the `Source` property.

**Solution:** Store your own reference to the `ObservableCollection` when creating the panel, then access it directly when you need to add/remove items.

```csharp
// In PrinterQueuePanel
public ObservableCollection<FileToPrint> Queue { get; }

public PrinterQueuePanel(string printerName)
{
    Queue = [];
    QueueList.SetSource(Queue);
}
```

### 2. "Collection was modified" Exception

**Problem:** When iterating over `newQueue` and adding to `currentQueue`, if they're the same reference (or related), you get a "collection was modified during enumeration" exception.

**Solution:** Create a snapshot copy before iterating:

```csharp
foreach (FileToPrint file in newQueue.ToArray())
{
    currentQueue.Add(file);
}
```

Alternative using Terminal.Gui's built-in methods:
```csharp
cachedPanel.QueueList.SuspendCollectionChangedEvent();
// ... modify collection ...
cachedPanel.QueueList.ResumeSuspendCollectionChangedEvent();
```

### 3. Compounding Items Bug

**Problem:** When passing the same collection reference multiple times, all items get re-added each time instead of just new items.

**Solution:** Either:
- Pass only NEW items to the update method
- Or clear the queue before replacing with full contents

---

## Code Review: PrinterQueueView

### Current Issues

1. **`GetCurrentQueue` returns disconnected empty collection on failure** - misleading behavior

2. **`CurrentLabelCount` should be private and use LINQ**

3. **Methods that should be private are public**

### Suggested Improvements

```csharp
// Cleaner GetCurrentQueue
public ObservableCollection<FileToPrint>? GetCurrentQueue(string printerName) =>
    _panels.TryGetValue(printerName, out var panel) ? panel.Queue : null;

// Use LINQ for label count
private int CurrentLabelCount(ObservableCollection<FileToPrint> queue) =>
    queue.Sum(f => f.LabelCount);

// Combined add/update pattern
public void AddOrUpdatePrinterPanel(string printerName, ObservableCollection<FileToPrint> newItems)
{
    if (!_panels.TryGetValue(printerName, out var panel))
    {
        Remove(QueueStatus);
        panel = new PrinterQueuePanel(printerName);
        _panels.Add(printerName, panel);
        PrinterPanelArrangement.SetPanelPos(_panels.Count - 1, panel);
        Add(panel);
    }

    foreach (var file in newItems)
        panel.Queue.Add(file);

    panel.UpdateLabelCount(panel.Queue.Sum(f => f.LabelCount));
}
```

---

## Proposed Clean Architecture

### Folder Structure

```
printFlowTui/
├── Program.cs              # Entry point only - minimal
├── App.cs                  # Application setup and main window
├── Config/
│   └── AppConfig.cs        # Centralized configuration
├── Models/
│   └── FileToPrint.cs
├── Services/
│   ├── IFileService.cs     # Interface for testability
│   ├── FileService.cs      # File loading implementation
│   └── PrinterService.cs   # Printer queue management
├── Views/
│   ├── MainView.cs         # Top-level container
│   ├── FileSelection/
│   │   ├── FileSelectionView.cs
│   │   └── FileListView.cs
│   ├── PrinterQueue/
│   │   ├── PrinterQueueView.cs
│   │   └── PrinterQueuePanel.cs
│   └── Dialogs/
│       └── OperationsDialog.cs
└── Extensions/
    └── ViewExtensions.cs   # Reusable Terminal.Gui helpers
```

### Key Patterns

#### 1. Minimal Program.cs

```csharp
namespace printFlowTui;

using Terminal.Gui.App;

internal class Program
{
    public static void Main()
    {
        using var application = Application.Create().Init();
        var app = new App();
        application.Run(app.MainWindow);
    }
}
```

#### 2. Centralized Configuration

```csharp
namespace printFlowTui.Config;

public static class AppConfig
{
    public static string LabelDataPath =>
        Environment.GetEnvironmentVariable("PRINTFLOW_DATA_PATH")
        ?? "/home/huckste/TestFolder/Label_Data_Load/";

    public static IReadOnlyList<PrinterInfo> Printers { get; } =
    [
        new("OCE4000", PrinterType.LargeFormat),
        new("OCE7400", PrinterType.LargeFormat),
        new("SATO100R", PrinterType.Label),
        new("SATO100R2", PrinterType.Label),
        new("SOLIDF90", PrinterType.Industrial),
    ];
}

public record PrinterInfo(string Name, PrinterType Type);

public enum PrinterType { LargeFormat, Label, Industrial }
```

#### 3. Base View Class

```csharp
namespace printFlowTui.Views;

using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;

public abstract class BaseView : View
{
    protected BaseView(string title)
    {
        Title = title;
        BorderStyle = LineStyle.Rounded;
        CanFocus = true;
    }

    /// <summary>
    /// Called after construction to load async data.
    /// </summary>
    public virtual Task InitializeAsync() => Task.CompletedTask;
}
```

#### 4. Service Interface Pattern

```csharp
namespace printFlowTui.Services;

using printFlowTui.Models;

public interface IFileService
{
    Task<IReadOnlyList<FileToPrint>> LoadFilesAsync(string path);
}

public class FileService : IFileService
{
    public async Task<IReadOnlyList<FileToPrint>> LoadFilesAsync(string path)
    {
        return await FetchFiles.LabelData(path);
    }
}
```

#### 5. Views Emit Events, App Coordinates

```csharp
// View emits event
public class FileSelectionView : BaseView
{
    public event EventHandler<FilesSelectedEventArgs>? FilesSelected;

    private void OnAccepting()
    {
        var marked = GetMarkedFiles();
        if (marked.Count == 0) return;
        FilesSelected?.Invoke(this, new FilesSelectedEventArgs(marked));
    }
}

// App.cs subscribes and coordinates
public class App
{
    public App()
    {
        _printFilesView.FileSelection.FilesSelected += OnFilesSelected;
        _operationsDialog.PrinterSelected += OnPrinterSelected;
    }

    private void OnFilesSelected(object? sender, FilesSelectedEventArgs e)
    {
        _selectedFiles = e.Files;
        ShowDialog();
    }
}
```

#### 6. Encapsulated Dialogs

```csharp
public class OperationsDialog : Dialog
{
    public event EventHandler<PrinterSelectedEventArgs>? PrinterSelected;
    public event EventHandler? DeleteRequested;
    public event EventHandler? Cancelled;

    // Dialog manages its own state machine internally
    private void ShowOperations() { /* ... */ }
    private void ShowPrinterSelection() { /* ... */ }
}
```

### Key Principles

| Principle | Implementation |
|-----------|----------------|
| Single Responsibility | Each class does one thing |
| Dependency Injection | `IFileService` injected for testability |
| Events over callbacks | Views emit events, parent coordinates |
| No blocking async | `InitializeAsync()` called after construction |
| Configuration centralized | `AppConfig` holds all constants |
| Encapsulated dialogs | Dialog handles its own state machine |
| C# conventions | `_privateField`, `PublicProperty`, `EventArgs` |
| Terminal.Gui patterns | `BaseView` for styling, proper `Dim`/`Pos` |

### Adding New Views

```csharp
// 1. Create view in Views/NewFeature/
public class NewFeatureView : BaseView
{
    public event EventHandler<SomeEventArgs>? SomethingHappened;

    public NewFeatureView() : base("New Feature")
    {
        // Setup UI
    }
}

// 2. Add to parent view or App.cs
// 3. Subscribe to events in App.cs
```

---

## Terminal.Gui ListView Quick Reference

- `SetSource<T>(ObservableCollection<T>)` - Sets the data source
- `Source` - Returns `IListDataSource` (NOT the original collection)
- `SuspendCollectionChangedEvent()` - Pause UI updates
- `ResumeSuspendCollectionChangedEvent()` - Resume UI updates
- `GetAllMarkedItems()` - Get indices of marked items
- `UnmarkAll()` - Clear all marks
- `SetNeedsDisplay()` - Force redraw
