using System.Buffers.Binary;
using System.Text;

namespace Pes2019MlEditor.Core.Models;

/// <summary>
/// Represents the 208-byte decrypted PES 2019 Save File Header.
/// </summary>
public sealed class FileHeader
{
    public const int HeaderSize = 208;

    public byte[] MysteryData { get; set; } = new byte[64];
    public uint DataSize { get; set; }
    public uint LogoSize { get; set; }
    public uint DescSize { get; set; }
    public uint SerialLength { get; set; }
    public byte[] Hash { get; set; } = new byte[64];
    public byte[] FileTypeBytes { get; set; } = new byte[32];
    public byte[] GameVersionBytes { get; set; } = new byte[32];

    public string FileTypeString
    {
        get => Encoding.ASCII.GetString(FileTypeBytes).TrimEnd('\0');
        set
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            Array.Clear(FileTypeBytes, 0, FileTypeBytes.Length);
            Array.Copy(bytes, FileTypeBytes, Math.Min(bytes.Length, FileTypeBytes.Length));
        }
    }

    public string GameVersionString
    {
        get => Encoding.ASCII.GetString(GameVersionBytes).TrimEnd('\0');
        set
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            Array.Clear(GameVersionBytes, 0, GameVersionBytes.Length);
            Array.Copy(bytes, GameVersionBytes, Math.Min(bytes.Length, GameVersionBytes.Length));
        }
    }

    public static FileHeader FromBytes(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < HeaderSize)
        {
            throw new ArgumentException($"Buffer too small for FileHeader. Expected at least {HeaderSize} bytes, got {buffer.Length}.", nameof(buffer));
        }

        var header = new FileHeader();
        buffer[..64].CopyTo(header.MysteryData);
        header.DataSize = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(64, 4));
        header.LogoSize = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(68, 4));
        header.DescSize = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(72, 4));
        header.SerialLength = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(76, 4));
        buffer.Slice(80, 64).CopyTo(header.Hash);
        buffer.Slice(144, 32).CopyTo(header.FileTypeBytes);
        buffer.Slice(176, 32).CopyTo(header.GameVersionBytes);

        return header;
    }

    public byte[] ToBytes()
    {
        var buffer = new byte[HeaderSize];
        MysteryData.AsSpan(0, Math.Min(64, MysteryData.Length)).CopyTo(buffer.AsSpan(0, 64));
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(64, 4), DataSize);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(68, 4), LogoSize);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(72, 4), DescSize);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(76, 4), SerialLength);
        Hash.AsSpan(0, Math.Min(64, Hash.Length)).CopyTo(buffer.AsSpan(80, 64));
        FileTypeBytes.AsSpan(0, Math.Min(32, FileTypeBytes.Length)).CopyTo(buffer.AsSpan(144, 32));
        GameVersionBytes.AsSpan(0, Math.Min(32, GameVersionBytes.Length)).CopyTo(buffer.AsSpan(176, 32));
        return buffer;
    }
}
