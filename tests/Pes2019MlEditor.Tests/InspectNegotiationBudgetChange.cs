using System.Buffers.Binary;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class InspectNegotiationBudgetChange
{
    private readonly ITestOutputHelper _output;
    public InspectNegotiationBudgetChange(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Check144F()
    {
        string dir = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save";
        var d0 = SaveFileManager.LoadSave(Path.Combine(dir, "ML00000000"));
        var d1 = SaveFileManager.LoadSave(Path.Combine(dir, "ML00000001"));

        int baseOffset = 0x00144F80;
        _output.WriteLine($"Offset      | Save0 Hex                        | Save1 Hex                        | Save0 Int       | Save1 Int");
        _output.WriteLine($"------------+----------------------------------+----------------------------------+-----------------+----------------");

        for (int i = 0; i < 160; i += 4)
        {
            int off = baseOffset + i;
            var s0 = d0.Data.AsSpan(off, 4);
            var s1 = d1.Data.AsSpan(off, 4);
            int v0 = BinaryPrimitives.ReadInt32LittleEndian(s0);
            int v1 = BinaryPrimitives.ReadInt32LittleEndian(s1);

            string mark = v0 != v1 ? " <== CHANGED!" : "";
            _output.WriteLine($"0x{off:X8}  | {Convert.ToHexString(s0),-32} | {Convert.ToHexString(s1),-32} | {v0,15:N0} | {v1,15:N0}{mark}");
        }
    }
}
