using System.Buffers.Binary;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class SearchBarcelonaData
{
    private readonly ITestOutputHelper _output;
    public SearchBarcelonaData(ITestOutputHelper output) => _output = output;

    [Fact]
    public void ScanBarcelona()
    {
        string path = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save\ML00000000";
        var desc = SaveFileManager.LoadSave(path);

        // Team ID 108 (0x0000006C)
        int targetTeamId = 108;
        _output.WriteLine($"Searching for occurrences of Team ID {targetTeamId} (0x{targetTeamId:X8})...");

        var matches = new List<int>();
        for (int i = 0; i <= desc.Data.Length - 4; i += 4)
        {
            int val = BinaryPrimitives.ReadInt32LittleEndian(desc.Data.AsSpan(i, 4));
            if (val == targetTeamId)
            {
                matches.Add(i);
            }
        }
        _output.WriteLine($"Found {matches.Count} matches for Team ID {targetTeamId}.");

        // Let's inspect large numbers (> 1,000,000 and < 1,000,000,000) that could be budgets
        _output.WriteLine("Scanning for typical budget-sized numbers (> 5M and < 500M) around first 20 team ID matches...");
        int inspected = 0;
        foreach (int m in matches)
        {
            if (inspected++ > 25) break;

            // Check ± 128 bytes
            int start = Math.Max(0, m - 128);
            int end = Math.Min(desc.Data.Length - 4, m + 128);

            for (int j = start; j <= end; j += 4)
            {
                int val = BinaryPrimitives.ReadInt32LittleEndian(desc.Data.AsSpan(j, 4));
                if (val >= 1_000_000 && val <= 500_000_000)
                {
                    _output.WriteLine($"Near TeamID at 0x{m:X8} -> Offset 0x{j:X8} (diff {j - m}): val = {val:N0}");
                }
            }
        }
    }
}
