using Pes2019MlEditor.Core.Crypto;
using Pes2019MlEditor.Core.Models;

namespace Pes2019MlEditor.Core.IO;

/// <summary>
/// Manages loading, saving, backup, and auto-detection of PES 2019 save files.
/// </summary>
public static class SaveFileManager
{
    /// <summary>
    /// Attempts to find the default PES 2019 save folder in My Documents.
    /// Returns null if not found.
    /// </summary>
    public static string? TryDetectSaveDirectory()
    {
        string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        
        string[] candidateVendors = ["HANO4U", "KONAMI"];
        foreach (var vendor in candidateVendors)
        {
            string vendorDir = Path.Combine(docs, vendor, "PRO EVOLUTION SOCCER 2019");
            if (Directory.Exists(vendorDir))
            {
                var saveDirs = Directory.GetDirectories(vendorDir, "save", SearchOption.AllDirectories);
                if (saveDirs.Length > 0)
                {
                    return saveDirs[0];
                }
            }
        }

        // Generic fallback scan across Documents for any folder containing PRO EVOLUTION SOCCER 2019\...\save
        try
        {
            var matchDirs = Directory.GetDirectories(docs, "PRO EVOLUTION SOCCER 2019", SearchOption.AllDirectories);
            foreach (var match in matchDirs)
            {
                var saveDirs = Directory.GetDirectories(match, "save", SearchOption.AllDirectories);
                if (saveDirs.Length > 0)
                {
                    return saveDirs[0];
                }
            }
        }
        catch
        {
            // Ignore permission exceptions on restricted subfolders
        }

        return null;
    }

    /// <summary>
    /// Lists all Master League save files in a given directory (e.g. ML00000000).
    /// </summary>
    public static string[] GetMasterLeagueSaves(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return [];
        }

        return Directory.GetFiles(directory, "ML*")
            .Where(f => !f.EndsWith(".bak", StringComparison.OrdinalIgnoreCase) &&
                        !f.Contains(".bak_", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f)
            .ToArray();
    }

    /// <summary>
    /// Loads and decrypts a save file.
    /// </summary>
    public static PesSaveDescriptor LoadSave(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Arquivo de save não encontrado.", filePath);
        }

        byte[] rawBytes = File.ReadAllBytes(filePath);
        return Pes19Crypto.Decrypt(rawBytes);
    }

    /// <summary>
    /// Creates a timestamped and standard .bak backup, then encrypts and writes the new save.
    /// </summary>
    public static string SaveWithBackup(string filePath, PesSaveDescriptor descriptor, bool createBackup = true)
    {
        if (createBackup && File.Exists(filePath))
        {
            string dir = Path.GetDirectoryName(filePath)!;
            string fileName = Path.GetFileName(filePath);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            string standardBak = Path.Combine(dir, $"{fileName}.bak");
            string timestampBak = Path.Combine(dir, $"{fileName}.bak_{timestamp}");

            File.Copy(filePath, standardBak, overwrite: true);
            File.Copy(filePath, timestampBak, overwrite: true);
        }

        byte[] encryptedBytes = Pes19Crypto.Encrypt(descriptor);

        // Safe atomic write
        string tempFile = filePath + ".tmp";
        File.WriteAllBytes(tempFile, encryptedBytes);
        File.Move(tempFile, filePath, overwrite: true);

        return filePath;
    }
}
