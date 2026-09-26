using System.Buffers.Binary;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class GenerateTestSaveSlot3
{
    private readonly ITestOutputHelper _output;
    public GenerateTestSaveSlot3(ITestOutputHelper output) => _output = output;

    [Fact]
    public void CreateModifiedSlot3()
    {
        string dir = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save";
        string sourcePath = Path.Combine(dir, "ML00000000");
        string targetPath = Path.Combine(dir, "ML00000002");

        var desc = SaveFileManager.LoadSave(sourcePath);

        int transferOffset = 0x00C1BABC;
        int currentRaw = BinaryPrimitives.ReadInt32LittleEndian(desc.Data.AsSpan(transferOffset, 4));
        _output.WriteLine($"Current Transfer at 0x{transferOffset:X8}: {currentRaw * 100:N0} € ({currentRaw})");

        // Let's set it to 150,000,000 € -> 150,000,000 / 100 = 1,500,000 (0x0016E360)
        int newTransferRaw = 1_500_000;
        BinaryPrimitives.WriteInt32LittleEndian(desc.Data.AsSpan(transferOffset, 4), newTransferRaw);

        // Also increase total balance at 0x00C1BACC to 300,000,000 € (3,000,000)
        int balanceOffset = 0x00C1BACC;
        int newBalanceRaw = 3_000_000;
        BinaryPrimitives.WriteInt32LittleEndian(desc.Data.AsSpan(balanceOffset, 4), newBalanceRaw);

        // Change save slot name/description to Slot 3
        string descText = System.Text.Encoding.UTF8.GetString(desc.Description);
        descText = descText.Replace("Liga Master 01", "Liga Master 03 (Mod)");
        var newDescBytes = System.Text.Encoding.UTF8.GetBytes(descText);
        Array.Copy(newDescBytes, desc.Description, Math.Min(newDescBytes.Length, desc.Description.Length));

        // Save to ML00000002
        SaveFileManager.SaveWithBackup(targetPath, desc, createBackup: false);
        _output.WriteLine($"Saved modified Slot 3 to: {targetPath}");
    }
}
