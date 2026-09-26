using System.Buffers.Binary;
using System.Numerics;
using Pes2019MlEditor.Core.Models;

namespace Pes2019MlEditor.Core.Crypto;

/// <summary>
/// Cryptographic engine for PES 2019 saves (EDIT and ML files).
/// Based on the Konami Mersenne Twister stream cipher.
/// </summary>
public static class Pes19Crypto
{
    public const int EncryptionHeaderSize = 320;

    /// <summary>
    /// PES 2019 Regular Edition 64-byte Master Key.
    /// </summary>
    public static readonly byte[] MasterKeyPes19 =
    [
        0xFD, 0x60, 0x4A, 0x3E, 0xFD, 0x69, 0x20, 0xD1,
        0x93, 0x92, 0x37, 0xD7, 0x60, 0xD8, 0x30, 0xEE,
        0x65, 0x66, 0xFD, 0x6C, 0xE6, 0x9E, 0x48, 0xF8,
        0x0A, 0x0D, 0xC1, 0x23, 0x7F, 0xAC, 0x89, 0x05,
        0x1D, 0xF8, 0x5A, 0x79, 0x10, 0x7E, 0xAD, 0x81,
        0xAC, 0xAE, 0x9A, 0x6A, 0xAB, 0x16, 0xA6, 0x81,
        0xC2, 0xD2, 0x18, 0xC0, 0xF4, 0xE6, 0x5C, 0x27,
        0x74, 0xF6, 0xC1, 0x9F, 0xF5, 0x01, 0x38, 0x72
    ];

    public static uint Rol(uint a, int shift) => BitOperations.RotateLeft(a, shift);

    public static uint Ror(uint a, int shift) => BitOperations.RotateRight(a, shift);

    public static void XorRepeatingBlocks(Span<byte> output64, ReadOnlySpan<byte> input)
    {
        for (int i = 0; i < input.Length; ++i)
        {
            output64[i & 63] ^= input[i];
        }
    }

    public static void XorWithLongParam(ReadOnlySpan<byte> input64, Span<byte> output64, ulong param)
    {
        for (int i = 0; i < 8; i++)
        {
            ulong val = BinaryPrimitives.ReadUInt64LittleEndian(input64.Slice(i * 8, 8));
            BinaryPrimitives.WriteUInt64LittleEndian(output64.Slice(i * 8, 8), val ^ param);
        }
    }

    public static void ReverseLongs(Span<byte> output64, ReadOnlySpan<byte> input64)
    {
        for (int i = 0; i < 8; ++i)
        {
            for (int j = 0; j < 8; ++j)
            {
                output64[i * 8 + j] = input64[i * 8 + 7 - j];
            }
        }
    }

    public static void CryptStream(Span<byte> output, ReadOnlySpan<byte> key64, ReadOnlySpan<byte> input)
    {
        int length = input.Length;
        var keyWords = new uint[16];
        for (int k = 0; k < 16; k++)
        {
            keyWords[k] = BinaryPrimitives.ReadUInt32LittleEndian(key64.Slice(k * 4, 4));
        }

        var mt = new MersenneTwister();
        mt.InitByArray(keyWords);

        uint c0 = mt.GenRandInt32();
        uint c1 = mt.GenRandInt32();
        uint c2 = mt.GenRandInt32();
        uint c3 = mt.GenRandInt32();

        int wordCount = length / 4;
        for (int i = 0; i < wordCount; ++i)
        {
            uint c4 = mt.GenRandInt32();
            uint inputWord = BinaryPrimitives.ReadUInt32LittleEndian(input.Slice(i * 4, 4));
            uint outWord = c4 ^ c3 ^ c2 ^ c1 ^ c0 ^ inputWord;
            BinaryPrimitives.WriteUInt32LittleEndian(output.Slice(i * 4, 4), outWord);

            c0 = Ror(c1, 15);
            c1 = Rol(c2, 11);
            c2 = Rol(c3, 7);
            c3 = Ror(c4, 13);
        }

        if ((length & 3) != 0)
        {
            int offset = length & ~3;
            int rem = length & 3;
            uint mask = mt.GenRandInt32() ^ c3 ^ c2 ^ c1 ^ c0;
            for (int b = 0; b < rem; b++)
            {
                output[offset + b] = (byte)(input[offset + b] ^ ((mask >> (b * 8)) & 0xFF));
            }
        }
    }

    public static void CryptHeader(Span<byte> output320, ReadOnlySpan<byte> input320, ReadOnlySpan<byte> masterKey64)
    {
        Span<byte> headerKey = stackalloc byte[64];
        Span<byte> shuffledMasterKey = stackalloc byte[64];

        input320.Slice(256, 64).CopyTo(headerKey);
        ReverseLongs(shuffledMasterKey, masterKey64);
        XorRepeatingBlocks(headerKey, shuffledMasterKey);

        CryptStream(output320, headerKey, input320);
        input320.Slice(256, 64).CopyTo(output320.Slice(256, 64));
    }

    public static PesSaveDescriptor Decrypt(ReadOnlySpan<byte> fileBytes, byte[]? masterKey = null)
    {
        masterKey ??= MasterKeyPes19;

        if (fileBytes.Length < EncryptionHeaderSize + FileHeader.HeaderSize)
        {
            throw new InvalidDataException("Arquivo muito pequeno para ser um save válido de PES 2019.");
        }

        var descriptor = new PesSaveDescriptor();
        CryptHeader(descriptor.EncryptionHeader, fileBytes[..EncryptionHeaderSize], masterKey);

        int offset = EncryptionHeaderSize;

        Span<byte> rollingKey = stackalloc byte[64];
        Span<byte> intermediateKey = stackalloc byte[64];

        descriptor.EncryptionHeader.AsSpan(0, 64).CopyTo(rollingKey);
        XorRepeatingBlocks(rollingKey, descriptor.EncryptionHeader.AsSpan(64, 256));

        // 1. FileHeader
        Span<byte> fileHeaderBuffer = stackalloc byte[FileHeader.HeaderSize];
        XorWithLongParam(rollingKey, intermediateKey, (ulong)FileHeader.HeaderSize);
        CryptStream(fileHeaderBuffer, intermediateKey, fileBytes.Slice(offset, FileHeader.HeaderSize));
        descriptor.FileHeader = FileHeader.FromBytes(fileHeaderBuffer);
        offset += FileHeader.HeaderSize;

        // 2. Description
        descriptor.Description = new byte[descriptor.FileHeader.DescSize];
        XorWithLongParam(rollingKey, intermediateKey, 0);
        CryptStream(descriptor.Description, intermediateKey, fileBytes.Slice(offset, (int)descriptor.FileHeader.DescSize));
        offset += (int)descriptor.FileHeader.DescSize;

        // 3. Logo
        descriptor.Logo = new byte[descriptor.FileHeader.LogoSize];
        XorWithLongParam(rollingKey, intermediateKey, 1);
        CryptStream(descriptor.Logo, intermediateKey, fileBytes.Slice(offset, (int)descriptor.FileHeader.LogoSize));
        offset += (int)descriptor.FileHeader.LogoSize;

        // 4. Data (payload)
        descriptor.Data = new byte[descriptor.FileHeader.DataSize];
        XorWithLongParam(rollingKey, intermediateKey, 2);
        CryptStream(descriptor.Data, intermediateKey, fileBytes.Slice(offset, (int)descriptor.FileHeader.DataSize));
        offset += (int)descriptor.FileHeader.DataSize;

        // 5. Serial
        int serialBytesCount = (int)descriptor.FileHeader.SerialLength * 2;
        descriptor.Serial = new byte[serialBytesCount];
        XorWithLongParam(rollingKey, intermediateKey, 3);
        CryptStream(descriptor.Serial, intermediateKey, fileBytes.Slice(offset, serialBytesCount));

        return descriptor;
    }

    public static byte[] Encrypt(PesSaveDescriptor descriptor, byte[]? masterKey = null)
    {
        masterKey ??= MasterKeyPes19;

        // Ensure header sizes match current buffers
        descriptor.FileHeader.DataSize = (uint)descriptor.Data.Length;
        descriptor.FileHeader.LogoSize = (uint)descriptor.Logo.Length;
        descriptor.FileHeader.DescSize = (uint)descriptor.Description.Length;
        descriptor.FileHeader.SerialLength = (uint)(descriptor.Serial.Length / 2);

        int totalSize = EncryptionHeaderSize
                        + FileHeader.HeaderSize
                        + descriptor.Description.Length
                        + descriptor.Logo.Length
                        + descriptor.Data.Length
                        + descriptor.Serial.Length;

        var result = new byte[totalSize];
        var outputSpan = result.AsSpan();

        // 0. Encryption Header
        CryptHeader(outputSpan[..EncryptionHeaderSize], descriptor.EncryptionHeader, masterKey);
        int offset = EncryptionHeaderSize;

        Span<byte> rollingKey = stackalloc byte[64];
        Span<byte> intermediateKey = stackalloc byte[64];

        descriptor.EncryptionHeader.AsSpan(0, 64).CopyTo(rollingKey);
        XorRepeatingBlocks(rollingKey, descriptor.EncryptionHeader.AsSpan(64, 256));

        // 1. FileHeader
        byte[] fileHeaderRaw = descriptor.FileHeader.ToBytes();
        XorWithLongParam(rollingKey, intermediateKey, (ulong)FileHeader.HeaderSize);
        CryptStream(outputSpan.Slice(offset, FileHeader.HeaderSize), intermediateKey, fileHeaderRaw);
        offset += FileHeader.HeaderSize;

        // 2. Description
        XorWithLongParam(rollingKey, intermediateKey, 0);
        CryptStream(outputSpan.Slice(offset, descriptor.Description.Length), intermediateKey, descriptor.Description);
        offset += descriptor.Description.Length;

        // 3. Logo
        XorWithLongParam(rollingKey, intermediateKey, 1);
        CryptStream(outputSpan.Slice(offset, descriptor.Logo.Length), intermediateKey, descriptor.Logo);
        offset += descriptor.Logo.Length;

        // 4. Data
        XorWithLongParam(rollingKey, intermediateKey, 2);
        CryptStream(outputSpan.Slice(offset, descriptor.Data.Length), intermediateKey, descriptor.Data);
        offset += descriptor.Data.Length;

        // 5. Serial
        XorWithLongParam(rollingKey, intermediateKey, 3);
        CryptStream(outputSpan.Slice(offset, descriptor.Serial.Length), intermediateKey, descriptor.Serial);

        return result;
    }
}
