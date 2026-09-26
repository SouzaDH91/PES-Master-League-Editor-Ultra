using System.Buffers.Binary;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class FindAccountBalanceTests
{
    private readonly ITestOutputHelper _output;
    public FindAccountBalanceTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void ScanExactAccountNumbers()
    {
        string path = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save\ML00000000";
        var desc = SaveFileManager.LoadSave(path);

        int[] targets = [
            202_679_900,  // Direitos transmissão
            137_023_000,  // Patrocínio
            19_297_400,   // Sócios torcedor
            2_098_800,    // Vendas de produtos
            4_000_000,    // Prêmio competição
            174_264_600,  // Salários do time
            -174_264_600  // Salários do time negativo
        ];

        string[] names = [
            "Direitos transmissão (202,679,900)",
            "Patrocínio (137,023,000)",
            "Sócios torcedor (19,297,400)",
            "Vendas de produtos (2,098,800)",
            "Prêmio competição (4,000,000)",
            "Salários do time (174,264,600)",
            "Salários do time negativo (-174,264,600)"
        ];

        for (int t = 0; t < targets.Length; t++)
        {
            int target = targets[t];
            var matches = new List<int>();
            for (int i = 0; i <= desc.Data.Length - 4; i++)
            {
                int val = BinaryPrimitives.ReadInt32LittleEndian(desc.Data.AsSpan(i, 4));
                if (val == target) matches.Add(i);
            }

            _output.WriteLine($"Target: {names[t]} -> Matches: {matches.Count}");
            foreach (var m in matches)
            {
                _output.WriteLine($"   Found at Offset 0x{m:X8}");
                // Let's dump nearby 128 bytes around match
                int start = Math.Max(0, m - 64);
                int end = Math.Min(desc.Data.Length - 4, m + 64);
                for (int j = start; j <= end; j += 4)
                {
                    int nearby = BinaryPrimitives.ReadInt32LittleEndian(desc.Data.AsSpan(j, 4));
                    if (nearby != 0)
                    {
                        _output.WriteLine($"      [0x{j:X8} / rel {j - m}]: {nearby:N0}");
                    }
                }
            }
        }
    }
}
