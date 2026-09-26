using System.Text;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class FindBarcelonaBlock
{
    private readonly ITestOutputHelper _output;
    public FindBarcelonaBlock(ITestOutputHelper output) => _output = output;

    [Fact]
    public void LocateBarca()
    {
        string path = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save\ML00000000";
        var desc = SaveFileManager.LoadSave(path);

        byte[] search = Encoding.ASCII.GetBytes("Barcelona");
        for (int i = 0; i <= desc.Data.Length - search.Length; i++)
        {
            if (desc.Data.AsSpan(i, search.Length).SequenceEqual(search))
            {
                _output.WriteLine($"Found 'Barcelona' at 0x{i:X8}!");
                // Dump 200 bytes around it
                int start = Math.Max(0, i - 16);
                for (int j = 0; j < 128; j += 16)
                {
                    _output.WriteLine($"0x{start + j:X8}: {Convert.ToHexString(desc.Data.AsSpan(start + j, 16))}");
                }
            }
        }
    }
}
