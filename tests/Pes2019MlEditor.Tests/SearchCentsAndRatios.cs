using System.Buffers.Binary;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class SearchCentsAndRatios
{
    private readonly ITestOutputHelper _output;
    public SearchCentsAndRatios(ITestOutputHelper output) => _output = output;

    [Fact]
    public void SearchVariations()
    {
        string path = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save\ML00000000";
        var desc = SaveFileManager.LoadSave(path);

        // In cents (x100) as int64
        long transCents = 30_908_600L * 100;
        long salCents = 5_999_400L * 100;

        _output.WriteLine($"Searching int64 cents: Trans={transCents}, Sal={salCents}");
        for (int i = 0; i <= desc.Data.Length - 8; i++)
        {
            long val = BinaryPrimitives.ReadInt64LittleEndian(desc.Data.AsSpan(i, 8));
            if (val == transCents) _output.WriteLine($"Found trans cents at 0x{i:X8}");
            if (val == salCents) _output.WriteLine($"Found sal cents at 0x{i:X8}");
        }

        // What if currency conversion?
        // Let's test EUR to GBP, USD, BRL, JPY exchange rates
        // Standard game rates: EUR/GBP ~ 0.85, EUR/USD ~ 1.15, EUR/BRL ~ 4.5, EUR/JPY ~ 130
        // What if the user screen is in EUR, but base is USD?
        // Or base is GBP?
        // Or base is JPY?
        // Let's search all 4-byte integers in desc.Data that when divided by 30908600 gives a sensible exchange rate (between 0.001 and 1000)
        // Wait, better yet, what if we search for byte patterns or look at where the manager finances are in the first 500KB of the save?
    }
}
