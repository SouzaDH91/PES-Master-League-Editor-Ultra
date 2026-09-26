using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class InspectDataHeader
{
    private readonly ITestOutputHelper _output;
    public InspectDataHeader(ITestOutputHelper output) => _output = output;

    [Fact]
    public void CheckDataHeader()
    {
        string path = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save\ML00000000";
        var desc = SaveFileManager.LoadSave(path);

        _output.WriteLine("First 160 bytes of desc.Data:");
        for (int i = 0; i < 160; i += 16)
        {
            _output.WriteLine($"0x{i:X2}: {Convert.ToHexString(desc.Data.AsSpan(i, 16))}");
        }
    }
}
