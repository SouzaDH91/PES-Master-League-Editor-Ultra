using System.Buffers.Binary;
using System.Text;
using Pes2019MlEditor.Core.Models;

namespace Pes2019MlEditor.Core.Squad;

/// <summary>
/// Service responsible for reading and writing Master League Squad data,
/// including Player Contracts, Salaries, Team Spirit, and Player Names.
/// </summary>
public static class MlSquadService
{
    public const int DefaultSquadOffset = 0x00C1D714;
    public const int PlayerRecordStride = 52; // 0x34 bytes
    public const int DefaultTeamSpiritOffset = 0x000100A8;
    public const int SecondaryTeamSpiritOffset = 0x000100D8;

    /// <summary>
    /// Reads the active squad list for the user's club from the decrypted Master League save data.
    /// Only sequential slots belonging to the active team roster (up to 32 players) are read.
    /// </summary>
    public static List<MlPlayerEntry> ReadSquad(byte[] data, IReadOnlyDictionary<int, string>? playerNames = null, int baseOffset = DefaultSquadOffset)
    {
        var list = new List<MlPlayerEntry>();
        if (data == null || baseOffset < 0 || baseOffset >= data.Length)
            return list;

        int currentOffset = baseOffset;
        int index = 0;

        while (currentOffset + PlayerRecordStride <= data.Length)
        {
            int slotIdx = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(currentOffset, 4));
            int pid = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(currentOffset + 4, 4));

            // Stop when we reach unallocated blocks or end of player table
            if (slotIdx == 65535 || (slotIdx == 0 && pid == 0))
            {
                break;
            }

            // Valid player records have valid positive slot index and player ID
            if (slotIdx > 0 && pid > 0 && pid < 5_000_000)
            {
                int salaryScaled = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(currentOffset + 8, 4));
                int contractFlag = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(currentOffset + 40, 4));
                byte[] contractEnd = data.AsSpan(currentOffset + 48, 4).ToArray();

                string name = string.Empty;
                if (playerNames != null && playerNames.TryGetValue(pid, out var resolvedName) && !string.IsNullOrWhiteSpace(resolvedName))
                {
                    name = resolvedName.Trim();
                }
                else
                {
                    name = GetDefaultKnownPlayerName(pid);
                }

                // Salary in PES:
                // For active club contracts, 1092 scaled ~= 13.3M EUR (~ 12,200 multiplier)
                // For transferred/negotiated targets (e.g. 510 scaled ~= 13.3M EUR, ~ 26,000 multiplier)
                // If scaled is smaller (e.g. <= 600) and it's a world-class player or contractFlag == 0, scale appropriately
                long multiplier = (contractFlag == 0 || salaryScaled <= 600) ? 26000L : 12200L;
                long estimatedEur = (long)salaryScaled * multiplier;

                string statusDesc = contractFlag == 1 ? "No Clube" : "Transferência / Alvo";

                list.Add(new MlPlayerEntry
                {
                    SquadIndex = index++,
                    SlotIndex = slotIdx,
                    PlayerId = pid,
                    Name = name,
                    SalaryScaled = salaryScaled,
                    SalaryEurEstimated = estimatedEur,
                    ContractFlag = contractFlag,
                    ContractEndRaw = contractEnd,
                    SaveOffset = currentOffset,
                    Status = statusDesc
                });
            }

            currentOffset += PlayerRecordStride;

            // Maximum combined roster & transfer target count
            if (list.Count >= 50)
                break;
        }

        return list;
    }

    /// <summary>
    /// Writes a new scaled salary value for a specific player entry.
    /// </summary>
    public static bool WritePlayerSalary(byte[] data, int saveOffset, int newSalaryScaled)
    {
        if (data == null || saveOffset < 0 || saveOffset + 12 > data.Length)
            return false;

        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(saveOffset + 8, 4), newSalaryScaled);
        return true;
    }

    /// <summary>
    /// Reads the team spirit (Espírito de Equipe, 1..99).
    /// </summary>
    public static byte ReadTeamSpirit(byte[] data, int offset = DefaultTeamSpiritOffset)
    {
        if (data == null || offset >= data.Length) return 0;
        return data[offset];
    }

    /// <summary>
    /// Writes the team spirit (Espírito de Equipe, 1..99) to the primary and secondary tactics offsets.
    /// </summary>
    public static bool WriteTeamSpirit(byte[] data, byte spirit, int primaryOffset = DefaultTeamSpiritOffset, int secondaryOffset = SecondaryTeamSpiritOffset)
    {
        if (data == null || primaryOffset >= data.Length) return false;
        
        spirit = Math.Clamp(spirit, (byte)1, (byte)99);
        data[primaryOffset] = spirit;

        if (secondaryOffset < data.Length)
        {
            data[secondaryOffset] = spirit;
        }

        return true;
    }

    /// <summary>
    /// Extracts all player names mapped by Player ID from a decrypted EDIT00000000 file.
    /// Scans the 188-byte stride player definition table.
    /// </summary>
    public static Dictionary<int, string> LoadPlayerNamesFromEdit(byte[] editData)
    {
        var names = new Dictionary<int, string>();
        if (editData == null || editData.Length < 1024)
            return names;

        int stride = 188;
        for (int o = 0x138; o + stride <= editData.Length; o += stride)
        {
            int pid = BinaryPrimitives.ReadInt32LittleEndian(editData.AsSpan(o, 4));
            if (pid is > 0 and < 5_000_000)
            {
                ReadOnlySpan<byte> nameSpan = editData.AsSpan(o + 52, Math.Min(46, editData.Length - (o + 52)));
                int nullIdx = nameSpan.IndexOf((byte)0);
                string raw = nullIdx >= 0 ? Encoding.UTF8.GetString(nameSpan[..nullIdx]) : Encoding.UTF8.GetString(nameSpan);
                string name = raw.Trim();
                // Valid player name must be non-empty, have letters, and not look like binary garbage
                if (!string.IsNullOrWhiteSpace(name) &&
                    name.Any(char.IsLetter) &&
                    name.All(c => char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsWhiteSpace(c)) &&
                    !names.ContainsKey(pid))
                {
                    names[pid] = name;
                }
            }
        }

        return names;
    }

    /// <summary>
    /// Extracts the club and league name from the decrypted save description header.
    /// E.g. "FC Barcelona / Liga Espanhola" -> Club: "FC Barcelona", League: "Liga Espanhola".
    /// </summary>
    public static (string Club, string League) ExtractTeamInfo(byte[] descriptionBytes)
    {
        if (descriptionBytes == null || descriptionBytes.Length == 0)
            return ("Clube Desconhecido", "");

        try
        {
            // Replace null bytes with spaces first so string functions work reliably
            string descText = Encoding.UTF8.GetString(descriptionBytes).Replace('\0', ' ');
            
            // First line contains save title and club/league
            int newlineIdx = descText.IndexOfAny(['\r', '\n']);
            string firstLine = newlineIdx >= 0 ? descText[..newlineIdx] : descText;

            // Look for " / " which separates Club and League
            int slashIdx = firstLine.IndexOf('/');
            if (slashIdx > 0)
            {
                string beforeSlash = firstLine[..slashIdx].Trim();
                string afterSlash = firstLine[(slashIdx + 1)..].Trim();

                // Strip leading "Liga Master XX" or multiple spaces
                int lastMultiSpace = beforeSlash.LastIndexOf("   ", StringComparison.Ordinal);
                string club = (lastMultiSpace >= 0 ? beforeSlash[(lastMultiSpace + 3)..].Trim() : beforeSlash).Trim();

                return (string.IsNullOrWhiteSpace(club) ? beforeSlash : club, afterSlash);
            }

            return (firstLine.Trim(), "");
        }
        catch
        {
            return ("Clube Desconhecido", "");
        }
    }

    /// <summary>
    /// Built-in fallback player name resolver for common and verified PES/FL players.
    /// </summary>
    public static string GetDefaultKnownPlayerName(int pid) => pid switch
    {
        40937 => "W. Szczęsny",
        47787 => "R. Lewandowski",
        103845 => "A. Christensen",
        110784 => "J. Koundé",
        147233 => "Alejandro Balde",
        108662 => "F. de Jong",
        110902 => "Dani Olmo",
        162114 => "Lamine Yamal",
        110644 => "Raphinha",
        144438 => "Ferran Torres",
        126337 => "Eric García",
        165696 => "Pau Cubarsí",
        155460 => "Marc Casadó",
        133157 => "Pedri",
        147235 => "Gavi",
        161548 => "Fermín López",
        141038 => "Joan García",
        138183 => "Gerard Martín",
        110815 => "Rodri",
        110718 => "K. Mbappé",
        151604 => "Héctor Fort",
        119835 => "Pau Víctor",
        171615 => "Guille Fernández",
        161886 => "Toni Fernández",
        161884 => "Quim Junyent",
        173584 => "Alexis Olmedo",
        161887 => "Landry Farré",
        171610 => "Noah Darvich",
        171614 => "Dani Rodríguez",
        113596 => "Unai Hernández",
        779999 => "J. Cuenca",
        8944 => "Karim Benzema",
        33185 => "M. Neuer",
        42316 => "A. Griezmann",
        44840 => "V. van Dijk",
        45945 => "İ. Gündoğan",
        _ => pid > 0 ? $"Jogador #{pid}" : "Livre / Vazio"
    };
}
