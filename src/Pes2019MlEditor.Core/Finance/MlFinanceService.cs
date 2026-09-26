using System.Buffers.Binary;

namespace Pes2019MlEditor.Core.Finance;

/// <summary>
/// Service to locate and modify budget records in decrypted PES 2019 Master League save data.
/// Automatically handles the PES internal currency scale (stored as value / 100).
/// </summary>
public static class MlFinanceService
{
    public const int DefaultTransferOffset = 0x00C1BABC;
    public const int DefaultBalanceOffset = 0x00C1BACC;

    /// <summary>
    /// Searches the decrypted data buffer for occurrences of target financial values.
    /// Supports both direct value and the PES 2019 standard / 100 scaled value.
    /// </summary>
    public static List<FinanceCandidate> FindCandidates(
        byte[] data,
        long primaryValue,
        long? secondaryValue = null,
        int maxProximity = 128)
    {
        var candidates = new List<FinanceCandidate>();
        if (data == null || data.Length < 4)
        {
            return candidates;
        }

        // Try both raw value and scaled / 100 value (PES 2019 stores values divided by 100)
        long[] primaryScales = primaryValue % 100 == 0
            ? [primaryValue / 100, primaryValue]
            : [primaryValue];

        long[]? secondaryScales = secondaryValue.HasValue
            ? (secondaryValue.Value % 100 == 0 ? [secondaryValue.Value / 100, secondaryValue.Value] : [secondaryValue.Value])
            : null;

        foreach (long pVal in primaryScales)
        {
            int targetVal32 = (int)pVal;
            bool isScaled = pVal != primaryValue;
            int multiplier = isScaled ? 100 : 1;

            var primaryOffsets = new List<int>();
            for (int i = 0; i <= data.Length - 4; i += 4)
            {
                int val = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(i, 4));
                if (val == targetVal32)
                {
                    primaryOffsets.Add(i);
                }
            }

            var secondaryOffsets = new List<int>();
            if (secondaryScales != null)
            {
                long sVal = isScaled ? secondaryScales[0] : secondaryScales[^1];
                int sTarget = (int)sVal;
                for (int i = 0; i <= data.Length - 4; i += 4)
                {
                    if (BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(i, 4)) == sTarget)
                    {
                        secondaryOffsets.Add(i);
                    }
                }
            }

            foreach (int pOffset in primaryOffsets)
            {
                int secOffset = -1;
                long secVal = 0;
                int bestDist = int.MaxValue;

                if (secondaryOffsets.Count > 0)
                {
                    foreach (int sOff in secondaryOffsets)
                    {
                        int dist = Math.Abs(pOffset - sOff);
                        if (dist > 0 && dist <= maxProximity && dist < bestDist)
                        {
                            bestDist = dist;
                            secOffset = sOff;
                            secVal = (long)BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(sOff, 4)) * multiplier;
                        }
                    }
                }
                else if (pOffset + 16 <= data.Length - 4)
                {
                    int nearby = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(pOffset + 16, 4));
                    if (nearby > 0)
                    {
                        secOffset = pOffset + 16;
                        secVal = (long)nearby * multiplier;
                        bestDist = 16;
                    }
                }

                candidates.Add(new FinanceCandidate
                {
                    Offset = pOffset,
                    FoundValue = (long)targetVal32 * multiplier,
                    SecondValueOffset = secOffset,
                    SecondValue = secVal,
                    ProximityToSecondValue = secOffset >= 0 ? bestDist : -1
                });
            }

            if (candidates.Count > 0)
            {
                break;
            }
        }

        // Always ensure known Master League Barcelona / Club finance offset is suggested if present
        if (DefaultTransferOffset <= data.Length - 4)
        {
            int currentTrans = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(DefaultTransferOffset, 4));
            if (currentTrans > 0 && !candidates.Any(c => c.Offset == DefaultTransferOffset))
            {
                int currentBal = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(DefaultBalanceOffset, 4));
                candidates.Insert(0, new FinanceCandidate
                {
                    Offset = DefaultTransferOffset,
                    FoundValue = (long)currentTrans * 100,
                    SecondValueOffset = DefaultBalanceOffset,
                    SecondValue = (long)currentBal * 100,
                    ProximityToSecondValue = 16
                });
            }
        }

        return candidates;
    }

    /// <summary>
    /// Reads a 32-bit integer scaled by 100 at a given offset.
    /// </summary>
    public static long ReadScaledBudget(byte[] data, int offset)
    {
        if (offset < 0 || offset + 4 > data.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset fora dos limites do arquivo.");
        }
        int val = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset, 4));
        return (long)val * 100;
    }

    /// <summary>
    /// Writes a budget value divided by 100 (PES 2019 internal format) at a given offset.
    /// </summary>
    public static void WriteScaledBudget(byte[] data, int offset, long fullAmount)
    {
        if (offset < 0 || offset + 4 > data.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset fora dos limites do arquivo.");
        }
        int scaled = (int)(fullAmount / 100);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(offset, 4), scaled);
    }

    public static int ReadInt32(byte[] data, int offset)
    {
        return BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset, 4));
    }

    public static void WriteInt32(byte[] data, int offset, int value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(offset, 4), value);
    }
}
