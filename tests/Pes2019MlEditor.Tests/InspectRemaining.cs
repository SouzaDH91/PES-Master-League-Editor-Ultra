using System.Buffers.Binary;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class InspectRemaining
{
    private readonly ITestOutputHelper _output;
    public InspectRemaining(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Check005D6290()
    {
        string path = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save\ML00000000";
        var desc = SaveFileManager.LoadSave(path);

        int off = 0x005D6290;
        _output.WriteLine($"Bytes at 0x{off:X8}:");
        for (int i = 0; i < 128; i += 16)
        {
            _output.WriteLine($"0x{off + i:X8}: {Convert.ToHexString(desc.Data.AsSpan(off + i, 16))}");
        }
    }
}
