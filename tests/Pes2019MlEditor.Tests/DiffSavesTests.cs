using System.Buffers.Binary;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class DiffSavesTests
{
    private readonly ITestOutputHelper _output;
    public DiffSavesTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void CompareSaves()
    {
        string dir = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save";
        string path0 = Path.Combine(dir, "ML00000000");
        string path1 = Path.Combine(dir, "ML00000001");

        var desc0 = SaveFileManager.LoadSave(path0);
        var desc1 = SaveFileManager.LoadSave(path1);

        _output.WriteLine($"Save0 Data Len: {desc0.Data.Length:N0}");
        _output.WriteLine($"Save1 Data Len: {desc1.Data.Length:N0}");

        int minLen = Math.Min(desc0.Data.Length, desc1.Data.Length);
        var diffOffsets = new List<int>();

        for (int i = 0; i <= minLen - 4; i += 4)
        {
            int v0 = BinaryPrimitives.ReadInt32LittleEndian(desc0.Data.AsSpan(i, 4));
            int v1 = BinaryPrimitives.ReadInt32LittleEndian(desc1.Data.AsSpan(i, 4));
            if (v0 != v1)
            {
                diffOffsets.Add(i);
            }
        }

        _output.WriteLine($"Total 4-byte differences: {diffOffsets.Count}");

        // Group into contiguous blocks
        var blocks = new List<(int Start, int Count)>();
        if (diffOffsets.Count > 0)
        {
            int curStart = diffOffsets[0];
            int curCount = 1;
            for (int k = 1; k < diffOffsets.Count; k++)
            {
                if (diffOffsets[k] == diffOffsets[k - 1] + 4)
                {
                    curCount++;
                }
                else
                {
                    blocks.Add((curStart, curCount));
                    curStart = diffOffsets[k];
                    curCount = 1;
                }
            }
            blocks.Add((curStart, curCount));
        }

        _output.WriteLine($"Difference blocks count: {blocks.Count}");

        foreach (var b in blocks)
        {
            _output.WriteLine($"--- Block at 0x{b.Start:X8} (Dwords: {b.Count}) ---");
            for (int off = b.Start; off < b.Start + Math.Min(b.Count * 4, 32); off += 4)
            {
                int val0 = BinaryPrimitives.ReadInt32LittleEndian(desc0.Data.AsSpan(off, 4));
                int val1 = BinaryPrimitives.ReadInt32LittleEndian(desc1.Data.AsSpan(off, 4));
                _output.WriteLine($"  0x{off:X8}: Save0 = {val0,12:N0} (0x{val0:X8}) -> Save1 = {val1,12:N0} (0x{val1:X8}) [diff = {val1 - val0:N0}]");
            }
        }
    }
}
