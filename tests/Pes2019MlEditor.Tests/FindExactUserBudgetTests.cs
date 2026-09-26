using System.Buffers.Binary;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class FindExactUserBudgetTests
{
    private readonly ITestOutputHelper _output;
    public FindExactUserBudgetTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void SearchUserBudget()
    {
        string path = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save\ML00000000";
        var desc = SaveFileManager.LoadSave(path);

        int transVal = 30_908_600;
        int salVal = 5_999_400;

        _output.WriteLine($"Target Transfer: {transVal:N0} (0x{transVal:X8})");
        _output.WriteLine($"Target Salary: {salVal:N0} (0x{salVal:X8})");

        // 1. Direct search 32-bit (every byte alignment)
        var transOffsets = new List<int>();
        var salOffsets = new List<int>();

        for (int i = 0; i <= desc.Data.Length - 4; i++)
        {
            int val = BinaryPrimitives.ReadInt32LittleEndian(desc.Data.AsSpan(i, 4));
            if (val == transVal) transOffsets.Add(i);
            if (val == salVal) salOffsets.Add(i);
        }

        _output.WriteLine($"Direct matches: Transfer = {transOffsets.Count}, Salary = {salOffsets.Count}");
        foreach (var t in transOffsets) _output.WriteLine($"Transfer at 0x{t:X8}");
        foreach (var s in salOffsets) _output.WriteLine($"Salary at 0x{s:X8}");

        // 2. Search divided by 100 or 1000
        int transValK = transVal / 1000;
        int salValK = salVal / 1000;
        _output.WriteLine($"Searching / 1000: TransferK = {transValK}, SalaryK = {salValK}");
        for (int i = 0; i <= desc.Data.Length - 4; i++)
        {
            int val = BinaryPrimitives.ReadInt32LittleEndian(desc.Data.AsSpan(i, 4));
            if (val == transValK) _output.WriteLine($"Transfer / 1000 match at 0x{i:X8}");
            if (val == salValK) _output.WriteLine($"Salary / 1000 match at 0x{i:X8}");
        }

        // 3. Search weekly (salary / 52)
        int salWeekly = salVal / 52;
        _output.WriteLine($"Searching weekly salary ({salWeekly}):");
        for (int i = 0; i <= desc.Data.Length - 4; i++)
        {
            int val = BinaryPrimitives.ReadInt32LittleEndian(desc.Data.AsSpan(i, 4));
            if (val == salWeekly) _output.WriteLine($"Salary weekly match at 0x{i:X8}");
        }

        // 4. Search double or float
        for (int i = 0; i <= desc.Data.Length - 4; i++)
        {
            float f = BitConverter.ToSingle(desc.Data, i);
            if (Math.Abs(f - transVal) < 1.0f) _output.WriteLine($"Float transfer match at 0x{i:X8}");
            if (Math.Abs(f - salVal) < 1.0f) _output.WriteLine($"Float salary match at 0x{i:X8}");
        }

        // 5. Search 64-bit int (long)
        for (int i = 0; i <= desc.Data.Length - 8; i++)
        {
            long val = BinaryPrimitives.ReadInt64LittleEndian(desc.Data.AsSpan(i, 8));
            if (val == transVal) _output.WriteLine($"Long transfer match at 0x{i:X8}");
            if (val == salVal) _output.WriteLine($"Long salary match at 0x{i:X8}");
        }
    }
}
