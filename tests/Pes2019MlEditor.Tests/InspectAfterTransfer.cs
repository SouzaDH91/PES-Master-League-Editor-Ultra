using System.Buffers.Binary;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class InspectAfterTransfer
{
    private readonly ITestOutputHelper _output;
    public InspectAfterTransfer(ITestOutputHelper output) => _output = output;

    [Fact]
    public void DumpAfter()
    {
        string dir = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save";
        var d0 = SaveFileManager.LoadSave(Path.Combine(dir, "ML00000000"));
        var d1 = SaveFileManager.LoadSave(Path.Combine(dir, "ML00000001"));

        int baseOffset = 0x00C1BB00;
        _output.WriteLine($"Offset      | Save0 Hex   | Save1 Hex   | Save0 Value * 100       | Save1 Value * 100       | Description");
        _output.WriteLine($"------------+-------------+-------------+-------------------------+-------------------------+--------------------");

        for (int i = 0; i < 256; i += 4)
        {
            int off = baseOffset + i;
            int v0 = BinaryPrimitives.ReadInt32LittleEndian(d0.Data.AsSpan(off, 4));
            int v1 = BinaryPrimitives.ReadInt32LittleEndian(d1.Data.AsSpan(off, 4));

            long real0 = (long)v0 * 100;
            long real1 = (long)v1 * 100;

            string mark = v0 != v1 ? " <== CHANGED!" : "";
            _output.WriteLine($"0x{off:X8}  | {v0:X8}    | {v1:X8}    | {real0,20:N0} €  | {real1,20:N0} €  {mark}");
        }
    }
}
