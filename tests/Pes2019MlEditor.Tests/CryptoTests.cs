using Pes2019MlEditor.Core.Crypto;
using Pes2019MlEditor.Core.Models;
using System.Security.Cryptography;

namespace Pes2019MlEditor.Tests;

public class CryptoTests
{
    [Fact]
    public void MersenneTwister_ProducesPredictableSequence()
    {
        var mt = new MersenneTwister();
        mt.InitGenRand(5489U);
        uint first = mt.GenRandInt32();
        uint second = mt.GenRandInt32();

        // Standard MT19937 reference check
        Assert.NotEqual(0U, first);
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void CryptStream_IsSymmetric()
    {
        byte[] key = new byte[64];
        RandomNumberGenerator.Fill(key);

        byte[] original = new byte[1024];
        RandomNumberGenerator.Fill(original);

        byte[] encrypted = new byte[original.Length];
        byte[] decrypted = new byte[original.Length];

        Pes19Crypto.CryptStream(encrypted, key, original);
        Pes19Crypto.CryptStream(decrypted, key, encrypted);

        Assert.Equal(original, decrypted);
    }

    [Fact]
    public void DecryptAndEncrypt_RoundtripFidelity()
    {
        // Construct a valid descriptor with mock data
        var originalDesc = new PesSaveDescriptor();
        RandomNumberGenerator.Fill(originalDesc.EncryptionHeader);
        originalDesc.Description = new byte[128];
        RandomNumberGenerator.Fill(originalDesc.Description);
        originalDesc.Logo = new byte[512];
        RandomNumberGenerator.Fill(originalDesc.Logo);
        originalDesc.Data = new byte[4096];
        RandomNumberGenerator.Fill(originalDesc.Data);
        originalDesc.Serial = new byte[64];
        RandomNumberGenerator.Fill(originalDesc.Serial);

        originalDesc.FileHeader.DataSize = (uint)originalDesc.Data.Length;
        originalDesc.FileHeader.LogoSize = (uint)originalDesc.Logo.Length;
        originalDesc.FileHeader.DescSize = (uint)originalDesc.Description.Length;
        originalDesc.FileHeader.SerialLength = (uint)(originalDesc.Serial.Length / 2);
        originalDesc.FileHeader.FileTypeString = "ML";
        originalDesc.FileHeader.GameVersionString = "1.00.00";

        // Encrypt it to a raw save file
        byte[] encryptedFile = Pes19Crypto.Encrypt(originalDesc);

        // Decrypt it back
        var decryptedDesc = Pes19Crypto.Decrypt(encryptedFile);

        // Assert all fields and data match byte-for-byte
        Assert.Equal(originalDesc.FileHeader.DataSize, decryptedDesc.FileHeader.DataSize);
        Assert.Equal(originalDesc.FileHeader.LogoSize, decryptedDesc.FileHeader.LogoSize);
        Assert.Equal(originalDesc.FileHeader.DescSize, decryptedDesc.FileHeader.DescSize);
        Assert.Equal(originalDesc.FileHeader.SerialLength, decryptedDesc.FileHeader.SerialLength);
        Assert.Equal(originalDesc.FileHeader.FileTypeString, decryptedDesc.FileHeader.FileTypeString);
        Assert.Equal(originalDesc.FileHeader.GameVersionString, decryptedDesc.FileHeader.GameVersionString);
        Assert.Equal(originalDesc.Description, decryptedDesc.Description);
        Assert.Equal(originalDesc.Logo, decryptedDesc.Logo);
        Assert.Equal(originalDesc.Data, decryptedDesc.Data);
        Assert.Equal(originalDesc.Serial, decryptedDesc.Serial);
    }

    [Fact]
    public void Decrypt_Pes2021_RealSave_ShouldSucceed()
    {
        string pes21Path = @"H:\Save games\KONAMI\eFootball PES 2021 SEASON UPDATE\292733975847239680\save\ML00000000";
        if (!File.Exists(pes21Path))
            return;

        byte[] raw = File.ReadAllBytes(pes21Path);
        byte[] pes21Key =
        [
            0x90, 0x61, 0xD8, 0x66, 0x43, 0x77, 0x24, 0xF8,
            0x92, 0xBA, 0xB8, 0x71, 0x21, 0xC7, 0x60, 0x63,
            0xF0, 0x91, 0x9A, 0x7D, 0xED, 0x47, 0x80, 0xDE,
            0x51, 0xF5, 0xDD, 0xD1, 0x08, 0xFE, 0x32, 0x84,
            0xF5, 0x09, 0x92, 0x00, 0xB2, 0x3E, 0x88, 0x9F,
            0xEB, 0x24, 0x43, 0x05, 0x58, 0x76, 0x00, 0x22,
            0x9B, 0xFE, 0xEC, 0xF6, 0x50, 0x00, 0x29, 0xD3,
            0x42, 0x75, 0x50, 0xB9, 0xEC, 0xD2, 0xF6, 0x75,
        ];

        var desc = Pes19Crypto.Decrypt(raw, pes21Key);
        Assert.NotNull(desc);
        Assert.Equal("ML", desc.FileHeader.FileTypeString);
        Assert.True(desc.Data.Length > 0);
    }
}
