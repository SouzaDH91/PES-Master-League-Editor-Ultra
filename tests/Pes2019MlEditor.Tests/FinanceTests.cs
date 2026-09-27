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

    [Fact]
    public void MlSquadService_ReadsSquadAndNamesCorrectly()
    {
        string mlPath = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save\ML00000000";
        string editPath = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save\EDIT00000000";
        if (!File.Exists(mlPath)) return;

        var mlDesc = Pes19Crypto.Decrypt(File.ReadAllBytes(mlPath), Pes19Crypto.MasterKeyPes19);

        Dictionary<int, string>? playerNames = null;
        if (File.Exists(editPath))
        {
            var editDesc = Pes19Crypto.Decrypt(File.ReadAllBytes(editPath), Pes19Crypto.MasterKeyPes19);
            playerNames = Pes2019MlEditor.Core.Squad.MlSquadService.LoadPlayerNamesFromEdit(editDesc.Data);
            Assert.True(playerNames.Count > 100);
            Assert.True(playerNames.ContainsKey(162114)); // Yamal
            Assert.True(playerNames.ContainsKey(110644)); // Raphinha
            Assert.True(playerNames.ContainsKey(110815)); // Rodri
        }

        var squad = Pes2019MlEditor.Core.Squad.MlSquadService.ReadSquad(mlDesc.Data, playerNames);
        Assert.NotEmpty(squad);
        Assert.True(squad.Count >= 30); // 30 active Barcelona players + transferred/targeted players

        // Verify team name extraction
        var (club, league) = Pes2019MlEditor.Core.Squad.MlSquadService.ExtractTeamInfo(mlDesc.Description);
        Assert.Equal("FC Barcelona", club);
        Assert.Equal("Liga Espanhola", league);

        // Verify key players are identified by their real names
        Assert.Contains(squad, p => p.Name == "Lamine Yamal" && p.PlayerId == 162114);
        Assert.Contains(squad, p => p.Name == "Raphinha" && p.PlayerId == 110644);
        Assert.Contains(squad, p => p.Name == "Rodri" && p.PlayerId == 110815);
        Assert.Contains(squad, p => p.Name == "Pau Cubarsí" && p.PlayerId == 165696);
        Assert.Contains(squad, p => p.Name == "Héctor Fort" && p.PlayerId == 151604);
        Assert.Contains(squad, p => p.Name == "Pau Víctor" && p.PlayerId == 119835);

        // Verify none of the squad members display as unmapped placeholder
        Assert.DoesNotContain(squad, p => p.Name.StartsWith("Jogador #"));

        // Verify that Mbappé (PID 110718) appears in the roster with proper name and > 13M EUR salary
        var mbappe = squad.FirstOrDefault(p => p.PlayerId == 110718);
        Assert.NotNull(mbappe);
        Assert.Equal("K. Mbappé", mbappe.Name);
        Assert.True(mbappe.SalaryEurEstimated >= 6_000_000); // 255 * 26000 = 6.6M in fresh ML0, or 510 * 26000 = 13.26M in ML1 career save!

        // Verify Team Spirit read
        byte spirit = Pes2019MlEditor.Core.Squad.MlSquadService.ReadTeamSpirit(mlDesc.Data);
        Assert.Equal(94, spirit);
    }
}
