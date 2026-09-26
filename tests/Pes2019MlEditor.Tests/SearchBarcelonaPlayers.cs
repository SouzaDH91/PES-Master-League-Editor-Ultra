using System.Buffers.Binary;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class SearchBarcelonaPlayers
{
    private readonly ITestOutputHelper _output;
    public SearchBarcelonaPlayers(ITestOutputHelper output) => _output = output;

    [Fact]
    public void FindMessi()
    {
        string path = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save\ML00000000";
        var desc = SaveFileManager.LoadSave(path);

        // Messi ID in PES is 7511 (0x1D57)
        int messiId = 7511;
        var matches = new List<int>();
        for (int i = 0; i <= desc.Data.Length - 4; i += 2)
        {
            int val = BinaryPrimitives.ReadInt32LittleEndian(desc.Data.AsSpan(i, 4));
            if (val == messiId) matches.Add(i);
        }

        _output.WriteLine($"Messi ID matches: {matches.Count}");
        foreach (var m in matches)
        {
            _output.WriteLine($"Offset 0x{m:X8}");
        }
    }
}
