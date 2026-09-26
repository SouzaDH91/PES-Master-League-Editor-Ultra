using Pes2019MlEditor.Core.Crypto;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class InspectRealSaveTests
{
    private readonly ITestOutputHelper _output;

    public InspectRealSaveTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void InspectUserSave()
    {
        string path = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save\ML00000000";
        if (!File.Exists(path))
        {
            _output.WriteLine("File not found");
            return;
        }

        var desc = SaveFileManager.LoadSave(path);
        _output.WriteLine($"FileType: '{desc.FileHeader.FileTypeString}'");
        _output.WriteLine($"GameVersion: '{desc.FileHeader.GameVersionString}'");
        _output.WriteLine($"DataLength: {desc.Data.Length:N0}");
        _output.WriteLine($"LogoLength: {desc.Logo.Length:N0}");

        Assert.Equal("ML", desc.FileHeader.FileTypeString);
    }
}
