namespace Pes2019MlEditor.Core.Models;

/// <summary>
/// Encapsulates all decrypted components of a PES 2019 save file.
/// </summary>
public sealed class PesSaveDescriptor
{
    public const int EncryptionHeaderSize = 320;

    /// <summary>
    /// 320-byte encryption header.
    /// </summary>
    public byte[] EncryptionHeader { get; set; } = new byte[EncryptionHeaderSize];

    /// <summary>
    /// 208-byte file header metadata.
    /// </summary>
    public FileHeader FileHeader { get; set; } = new();

    /// <summary>
    /// Save description text / metadata.
    /// </summary>
    public byte[] Description { get; set; } = [];

    /// <summary>
    /// Save thumbnail / logo PNG image.
    /// </summary>
    public byte[] Logo { get; set; } = [];

    /// <summary>
    /// Decrypted game payload (data.dat) containing the actual save data (teams, players, finances, etc.).
    /// </summary>
    public byte[] Data { get; set; } = [];

    /// <summary>
    /// Serial / version string data.
    /// </summary>
    public byte[] Serial { get; set; } = [];
}
