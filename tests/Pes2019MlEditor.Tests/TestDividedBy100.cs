using System.Buffers.Binary;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class TestDividedBy100
{
    private readonly ITestOutputHelper _output;
    public TestDividedBy100(ITestOutputHelper output) => _output = output;

    [Fact]
    public void CheckDividedBy100()
    {
        string dir = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save";
        var d0 = SaveFileManager.LoadSave(Path.Combine(dir, "ML00000000"));
        var d1 = SaveFileManager.LoadSave(Path.Combine(dir, "ML00000001"));

        int targetTrans = 309_086; // 30,908,600 / 100
        int targetSal0 = 59_994;   // 5,999,400 / 100
        int targetSal1 = -71_358;  // -7,135,800 / 100

        _output.WriteLine($"Searching in Save0 for Trans: {targetTrans} and Sal: {targetSal0}");
        var trans0 = new List<int>();
        var sal0 = new List<int>();
        for (int i = 0; i <= d0.Data.Length - 4; i++)
        {
            int val = BinaryPrimitives.ReadInt32LittleEndian(d0.Data.AsSpan(i, 4));
            if (val == targetTrans) trans0.Add(i);
            if (val == targetSal0) sal0.Add(i);
        }

        _output.WriteLine($"Save0 matches: Trans={trans0.Count}, Sal={sal0.Count}");
        foreach (var t in trans0) _output.WriteLine($"Trans at 0x{t:X8}");
        foreach (var s in sal0) _output.WriteLine($"Sal at 0x{s:X8}");

        _output.WriteLine($"\nSearching in Save1 for Trans: {targetTrans} and Sal: {targetSal1}");
        var trans1 = new List<int>();
        var sal1 = new List<int>();
        for (int i = 0; i <= d1.Data.Length - 4; i++)
        {
            int val = BinaryPrimitives.ReadInt32LittleEndian(d1.Data.AsSpan(i, 4));
            if (val == targetTrans) trans1.Add(i);
            if (val == targetSal1) sal1.Add(i);
        }

        _output.WriteLine($"Save1 matches: Trans={trans1.Count}, Sal={sal1.Count}");
        foreach (var t in trans1) _output.WriteLine($"Trans at 0x{t:X8}");
        foreach (var s in sal1) _output.WriteLine($"Sal at 0x{s:X8}");

        // Also search for the other numbers:
        int[] otherNumbers = [2_026_799, 1_370_230, 192_974, 20_988, 40_000, 1_742_646];
        string[] otherNames = ["Transmissao (2,026,799)", "Patrocinio (1,370,230)", "Socios (192,974)", "Produtos (20,988)", "Premio (40,000)", "Salarios (1,742,646)"];

        for (int k = 0; k < otherNumbers.Length; k++)
        {
            int num = otherNumbers[k];
            int count = 0;
            for (int i = 0; i <= d0.Data.Length - 4; i++)
            {
                if (BinaryPrimitives.ReadInt32LittleEndian(d0.Data.AsSpan(i, 4)) == num)
                {
                    _output.WriteLine($"Match for {otherNames[k]} at 0x{i:X8}");
                    count++;
                }
            }
            _output.WriteLine($"{otherNames[k]}: {count} matches");
        }
    }
}
