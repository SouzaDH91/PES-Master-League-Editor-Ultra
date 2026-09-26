using System.Buffers.Binary;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class FindSalaryCap
{
    private readonly ITestOutputHelper _output;
    public FindSalaryCap(ITestOutputHelper output) => _output = output;

    [Fact]
    public void SearchCap()
    {
        string dir = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save";
        var d0 = SaveFileManager.LoadSave(Path.Combine(dir, "ML00000000"));

        int cap = 180_264_000;
        _output.WriteLine($"Searching for Salary Cap: {cap:N0} (0x{cap:X8})");

        var matches = new List<int>();
        for (int i = 0; i <= d0.Data.Length - 4; i++)
        {
            int val = BinaryPrimitives.ReadInt32LittleEndian(d0.Data.AsSpan(i, 4));
            if (val == cap) matches.Add(i);
        }

        _output.WriteLine($"Matches for {cap:N0}: {matches.Count}");
        foreach (var m in matches)
        {
            _output.WriteLine($"Found at Offset 0x{m:X8}!");
            // Check nearby values
            for (int j = m - 32; j <= m + 32; j += 4)
            {
                int val = BinaryPrimitives.ReadInt32LittleEndian(d0.Data.AsSpan(j, 4));
                _output.WriteLine($"   0x{j:X8} ({j - m:+0;-0}): {val,15:N0} (0x{val:X8})");
            }
        }

        // Also search for 180,264 or divided by 1000
        int capK = cap / 1000;
        _output.WriteLine($"\nSearching for {capK} (/ 1000):");
        for (int i = 0; i <= d0.Data.Length - 4; i++)
        {
            int val = BinaryPrimitives.ReadInt32LittleEndian(d0.Data.AsSpan(i, 4));
            if (val == capK) _output.WriteLine($"Cap/1000 at 0x{i:X8}");
        }
    }
}
