using System.Buffers.Binary;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class InspectSections
{
    private readonly ITestOutputHelper _output;
    public InspectSections(ITestOutputHelper output) => _output = output;

    [Fact]
    public void ListSections()
    {
        string path = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save\ML00000000";
        var desc = SaveFileManager.LoadSave(path);

        _output.WriteLine("First 256 bytes in DWORDs:");
        for (int i = 0; i < 256; i += 16)
        {
            var span = desc.Data.AsSpan(i, 16);
            int d0 = BinaryPrimitives.ReadInt32LittleEndian(span[0..4]);
            int d1 = BinaryPrimitives.ReadInt32LittleEndian(span[4..8]);
            int d2 = BinaryPrimitives.ReadInt32LittleEndian(span[8..12]);
            int d3 = BinaryPrimitives.ReadInt32LittleEndian(span[12..16]);
            _output.WriteLine($"0x{i:X2}: [0x{d0:X8}, 0x{d1:X8}, 0x{d2:X8}, 0x{d3:X8}] | ({d0}, {d1}, {d2}, {d3})");
        }
    }
}
