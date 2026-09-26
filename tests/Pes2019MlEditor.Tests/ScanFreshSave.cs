using System.Buffers.Binary;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class ScanFreshSave
{
    private readonly ITestOutputHelper _output;
    public ScanFreshSave(ITestOutputHelper output) => _output = output;

    [Fact]
    public void ScanNewSave()
    {
        string path = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save\ML00000000";
        var desc = SaveFileManager.LoadSave(path);

        int transVal = 30_908_600;
        int salVal = 5_999_400;

        _output.WriteLine($"Target Transfer: {transVal} (0x{transVal:X8})");
        _output.WriteLine($"Target Salary: {salVal} (0x{salVal:X8})");

        var transMatches = new List<int>();
        var salMatches = new List<int>();

        for (int i = 0; i <= desc.Data.Length - 4; i++)
        {
            int val = BinaryPrimitives.ReadInt32LittleEndian(desc.Data.AsSpan(i, 4));
            if (val == transVal) transMatches.Add(i);
            if (val == salVal) salMatches.Add(i);
        }

        _output.WriteLine($"Exact matches: Transfer={transMatches.Count}, Salary={salMatches.Count}");
        foreach (var t in transMatches) _output.WriteLine($"Transfer match at 0x{t:X8}");
        foreach (var s in salMatches) _output.WriteLine($"Salary match at 0x{s:X8}");

        // Also check if any integer is within 5% of 30,908,600 and another within 5% of 5,999,400 within 64 bytes
        for (int i = 0; i <= desc.Data.Length - 64; i += 4)
        {
            int v1 = BinaryPrimitives.ReadInt32LittleEndian(desc.Data.AsSpan(i, 4));
            if (Math.Abs(v1 - transVal) < 100_000)
            {
                _output.WriteLine($"Close transfer candidate at 0x{i:X8}: {v1:N0} (diff: {v1 - transVal})");
                for (int j = i - 64; j <= i + 64; j += 4)
                {
                    if (j < 0 || j > desc.Data.Length - 4 || j == i) continue;
                    int v2 = BinaryPrimitives.ReadInt32LittleEndian(desc.Data.AsSpan(j, 4));
                    if (v2 > 1_000_000 && v2 < 50_000_000)
                    {
                        _output.WriteLine($"   Nearby at 0x{j:X8} (offset diff {j - i}): {v2:N0}");
                    }
                }
            }
        }
    }
}
