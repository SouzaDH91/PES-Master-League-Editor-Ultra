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
}
