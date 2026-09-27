namespace Pes2019MlEditor.Core.Squad;

/// <summary>
/// Represents an active squad player in the Master League save file.
/// </summary>
public sealed class MlPlayerEntry
{
    /// <summary>
    /// Index in the squad list (0..N).
    /// </summary>
    public int SquadIndex { get; set; }

    /// <summary>
    /// Player slot index (e.g. 1303, 1311, 1331).
    /// </summary>
    public int SlotIndex { get; set; }

    /// <summary>
    /// PES Player ID (e.g. 110644 for Raphinha, 162114 for Yamal, 110815 for Rodri).
    /// </summary>
    public int PlayerId { get; set; }

    /// <summary>
    /// Resolved player name (from EDIT00000000 or database/fallback).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Scaled salary value stored in the squad table (e.g. 1053, 1029, 1092).
    /// </summary>
    public int SalaryScaled { get; set; }

    /// <summary>
    /// Approximate annual salary in Euros (€) calculated from the scaled salary and in-game multiplier.
    /// </summary>
    public long SalaryEurEstimated { get; set; }

    /// <summary>
    /// Contract status flag (byte at offset + 40, usually 1 for active contract).
    /// </summary>
    public int ContractFlag { get; set; }

    /// <summary>
    /// Raw contract end marker (4 bytes at offset + 48).
    /// </summary>
    public byte[] ContractEndRaw { get; set; } = new byte[4];

    /// <summary>
    /// Exact byte offset where this 52-byte record begins in the decrypted save data.
    /// </summary>
    public int SaveOffset { get; set; }

    /// <summary>
    /// Contract or squad status description (e.g. "Elenco Principal", "Transferido / Alvo").
    /// </summary>
    public string Status { get; set; } = "No Clube";
}
