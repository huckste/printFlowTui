using System.Collections.ObjectModel;
using printFlowTui.Models;
using printFlowTui.Services;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace printFlowTui.Views;

public class FileSelectionView : BaseView
{
    public ListView FileList { get; }
    public Label StatusLabel { get; }
    public ObservableCollection<FileToPrint> AvailableFiles { get; }
    public bool _initalized;

    public event EventHandler<FilesSelectedEventArgs>? FilesSelected;

    public FileSelectionView()
    {
        Title = "File Selection";
        Width = Dim.Auto(minimumContentDim: 25);
        Height = Dim.Auto();
        CanFocus = true;

        FileList = new ListView
        {
            X = 1,
            Y = 1,
            Width = Dim.Auto(),
            Height = Dim.Auto(),
            MarkMultiple = true,
            ShowMarks = true,
        };

        StatusLabel = new Label { X = 1, Y = Pos.Bottom(FileList) + 1 };

        AvailableFiles = [];
        FileList.SetSource(AvailableFiles);

        FileList.Accepting += (s, e) =>
        {
            OnAccepting();
            e.Handled = true;
        };

        Add(FileList, StatusLabel);
    }

    public override async Task InitializeAsync()
    {
        if (_initalized)
            return;

        _initalized = true;
        StatusLabel.Text = "Loading Files...";
        var files = await FetchFiles.LabelData("/home/huckste/TestFolder/Label_Data_Load/");

        foreach (var file in files)
        {
            AvailableFiles.Add(file);
        }

        UpdateStatus();
    }

    public void UpdateStatus() =>
        StatusLabel.Text = $"Found {AvailableFiles.Sum(f => f.LabelCount)} Files";

    public class FilesSelectedEventArgs(IEnumerable<int> markedIndices) : EventArgs
    {
        public IEnumerable<int> MarkedIndices { get; } = markedIndices;
    }

    private void OnAccepting()
    {
        var marked = FileList.GetAllMarkedItems();
        FileList.UnmarkAll();

        FilesSelected?.Invoke(this, new FilesSelectedEventArgs(marked));
    }

    public List<FileToPrint>? RemoveMarkedFiles(IEnumerable<int>? markedIndices)
    {
        if (markedIndices == null)
            return null;

        var queue = new List<FileToPrint>();

        foreach (var idx in markedIndices)
        {
            queue.Add(AvailableFiles[idx]);
        }

        foreach (var idx in markedIndices.OrderByDescending(i => i))
        {
            AvailableFiles.RemoveAt(idx);
        }

        return queue;
    }
}
