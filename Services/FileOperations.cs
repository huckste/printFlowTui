namespace printFlowTui.Services;

using System.IO;
using printFlowTui.Models;

public class FetchFiles
{
    public static async Task<List<FileToPrint>> LabelData(string folderPath)
    {
        var files = Directory.GetFiles(folderPath);
        var list = new List<FileToPrint>();
        int fileID = 1;

        foreach (var file in files)
        {
            list.Add(
                new FileToPrint
                {
                    FileName = file.Split('/')[^1],
                    Path = file,
                    ID = fileID,
                    LabelCount = await LabelCountAysnc(file),
                    Queued = false,
                }
            );

            fileID++;
        }

        return list;
    }

    public static async Task<int> LabelCountAysnc(string filePath)
    {
        var lines = await File.ReadAllLinesAsync(filePath);
        return lines.Length;
    }

    public static async Task<List<Printer>> Printers(string path)
    {
        var printers = Directory.GetDirectories(path);

        var printerList = new List<Printer>();

        int printerID = 1;

        foreach (var printer in printers)
        {
            printerList.Add(
                new Printer
                {
                    PrinterName = printer.Split("-")[^1],
                    Path = printer,
                    ID = printerID,
                }
            );
            printerID++;
        }

        return printerList;
    }
}
