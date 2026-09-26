using System.Text;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class InspectEditSave
{
    private readonly ITestOutputHelper _output;
    public InspectEditSave(ITestOutputHelper output) => _output = output;

    [Fact]
    public void InspectEdit()
    {
        string path = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save\EDIT00000000";
        var desc = SaveFileManager.LoadSave(path);

        _output.WriteLine($"FileType: '{desc.FileHeader.FileTypeString}'");
        _output.WriteLine($"GameVersion: '{desc.FileHeader.GameVersionString}'");
        _output.WriteLine($"DataSize: {desc.Data.Length:N0}");
        _output.WriteLine($"First 64 bytes: {Convert.ToHexString(desc.Data.AsSpan(0, 64))}");

        // Search for known strings in EDIT, like "BARCELONA" or "MESSI"
        string ascii = Encoding.ASCII.GetString(desc.Data);
        int barcaPos = ascii.IndexOf("Barcelona", StringComparison.OrdinalIgnoreCase);
        int messiPos = ascii.IndexOf("Messi", StringComparison.OrdinalIgnoreCase);
        _output.WriteLine($"EDIT search: Barcelona index = {barcaPos}, Messi index = {messiPos}");
    }
}
