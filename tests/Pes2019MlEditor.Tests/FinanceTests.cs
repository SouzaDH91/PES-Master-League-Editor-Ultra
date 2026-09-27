using Pes2019MlEditor.Core.Crypto;
using Pes2019MlEditor.Core.Finance;
using Pes2019MlEditor.Core.IO;
using Pes2019MlEditor.Core.Models;

namespace Pes2019MlEditor.Tests;

public class FinanceTests
{
    [Fact]
    public void MlFinanceService_FindsExactValueAndProximity()
    {
        byte[] mockData = new byte[2048];

        int transferBudget = 45000000; // 45M
        int salaryBudget = 7500000;   // 7.5M

        int offsetTransfer = 256;
        int offsetSalary = 264;

        MlFinanceService.WriteInt32(mockData, offsetTransfer, transferBudget);
        MlFinanceService.WriteInt32(mockData, offsetSalary, salaryBudget);

        var matches = MlFinanceService.FindCandidates(mockData, transferBudget, salaryBudget);

        Assert.NotEmpty(matches);
        var best = matches[0];
        Assert.Equal(offsetTransfer, best.Offset);
        Assert.Equal(offsetSalary, best.SecondValueOffset);
        Assert.Equal(8, best.ProximityToSecondValue);

        // Edit value
        MlFinanceService.WriteInt32(mockData, offsetTransfer, 99000000);
        Assert.Equal(99000000, MlFinanceService.ReadInt32(mockData, offsetTransfer));
    }

    [Fact]
    public void SaveFileManager_DetectsOrReturnsSaveDir()
    {
        // TryDetectSaveDirectory should not throw
        var dir = SaveFileManager.TryDetectSaveDirectory();
        // It either returns null or a valid directory string
        if (dir != null)
        {
            Assert.True(Directory.Exists(dir) || dir.Contains("KONAMI"));
        }
    }

    [Fact]
    public void MlCalendarService_ReadsAndWritesDateProperly()
    {
        string save0Path = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save\ML00000000";
        if (!File.Exists(save0Path)) return;

        var desc = Pes19Crypto.Decrypt(File.ReadAllBytes(save0Path), Pes19Crypto.MasterKeyPes19);
        var date = Pes2019MlEditor.Core.Calendar.MlCalendarService.ReadDate(desc.Data);

        Assert.NotNull(date);
        Assert.Equal(2018, date.Value.Year);
        Assert.Equal(8, date.Value.Month);
        Assert.Equal(13, date.Value.Day);

        // Test writing new date (e.g. 01/09/2018)
        var newDate = new DateTime(2018, 9, 1);
        bool success = Pes2019MlEditor.Core.Calendar.MlCalendarService.WriteDate(desc, newDate);
        Assert.True(success);

        var verifyDate = Pes2019MlEditor.Core.Calendar.MlCalendarService.ReadDate(desc.Data);
        Assert.Equal(newDate, verifyDate);
    }
}
