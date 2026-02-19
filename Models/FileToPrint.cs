namespace printFlowTui.Models;

public class FileToPrint
{
    public required string FileName { get; set; }
    public required string Path { get; set; }
    public required int ID { get; set; }
    public required int LabelCount { get; set; }
    public bool Queued { get; set; }

    public override string ToString() => $"{FileName} ({LabelCount})";
}
