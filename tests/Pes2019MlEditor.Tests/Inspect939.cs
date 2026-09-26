using System.Buffers.Binary;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class Inspect939
{
    private readonly ITestOutputHelper _output;
    public Inspect939(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Dump939()
    {
        string path = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save\ML00000000";
        var desc = SaveFileManager.LoadSave(path);

        int start = 0x00939A80;
        for (int i = 0; i < 256; i += 4)
        {
            int off = start + i;
            int val = BinaryPrimitives.ReadInt32LittleEndian(desc.Data.AsSpan(off, 4));
            _output.WriteLine($"0x{off:X8} (+{i,3}): {val,12:N0} (0x{val:X8})");
        }
    }
}
