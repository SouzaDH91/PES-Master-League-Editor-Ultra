using System.Text;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class AnalyzeUserSave
{
    private readonly ITestOutputHelper _output;
    public AnalyzeUserSave(ITestOutputHelper output) => _output = output;

    [Fact]
    public void ScanSave()
    {
        string path = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save\ML00000000";
        var desc = SaveFileManager.LoadSave(path);
        string descStr = Encoding.UTF8.GetString(desc.Description).Trim('\0');
        _output.WriteLine($"Description: {descStr}");
        _output.WriteLine($"Data start: {Convert.ToHexString(desc.Data.AsSpan(0, 64))}");
    }
}
