using System.Buffers.Binary;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class InspectTableB53
{
    private readonly ITestOutputHelper _output;
    public InspectTableB53(ITestOutputHelper output) => _output = output;

    [Fact]
    public void DumpArea()
    {
        string path = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save\ML00000000";
        var desc = SaveFileManager.LoadSave(path);

        int baseOffset = 0x00B53900;
        _output.WriteLine($"Dumping 512 bytes around 0x{baseOffset:X8}:");

        for (int i = 0; i < 512; i += 16)
        {
            int off = baseOffset + i;
            var span = desc.Data.AsSpan(off, 16);
            string hex = Convert.ToHexString(span);
            int v0 = BinaryPrimitives.ReadInt32LittleEndian(span[0..4]);
            int v1 = BinaryPrimitives.ReadInt32LittleEndian(span[4..8]);
            int v2 = BinaryPrimitives.ReadInt32LittleEndian(span[8..12]);
            int v3 = BinaryPrimitives.ReadInt32LittleEndian(span[12..16]);
            _output.WriteLine($"0x{off:X8}: {hex} | {v0,12:N0} | {v1,12:N0} | {v2,12:N0} | {v3,12:N0}");
        }
    }
}
