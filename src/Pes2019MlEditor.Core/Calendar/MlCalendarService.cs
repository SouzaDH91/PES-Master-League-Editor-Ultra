using System.Buffers.Binary;
using System.Text.RegularExpressions;
using Pes2019MlEditor.Core.Models;

namespace Pes2019MlEditor.Core.Calendar;

/// <summary>
/// Service responsible for reading and writing Master League career calendar dates.
/// </summary>
public static class MlCalendarService
{
    // Primary calendar date block in ML save data (e.g. 0x009F21CC)
    public const int DefaultDateOffset = 0x009F21CC;

    /// <summary>
    /// Reads the current career date from the decrypted save data.
    /// Returns null if date signature is not found.
    /// </summary>
    public static DateTime? ReadDate(byte[] data, int offset = DefaultDateOffset)
    {
        if (data == null || offset + 4 > data.Length)
            return null;

        ushort year = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset, 2));
        byte month = data[offset + 2];
        byte day = data[offset + 3];

        if (year is >= 2018 and <= 2045 && month is >= 1 and <= 12 && day is >= 1 and <= 31)
        {
            try
            {
                return new DateTime(year, month, day);
            }
            catch
            {
                return null;
            }
        }

        // Fallback: search for date marker
        int foundOffset = FindDateOffset(data);
        if (foundOffset != -1)
        {
            ushort y = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(foundOffset, 2));
            byte m = data[foundOffset + 2];
            byte d = data[foundOffset + 3];
            try
            {
                return new DateTime(y, m, d);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    /// <summary>
    /// Writes the new career date to the save data at both the primary and secondary date offsets,
    /// and synchronously updates the human-readable Description string in the save descriptor.
    /// </summary>
    public static bool WriteDate(PesSaveDescriptor save, DateTime newDate, int offset = DefaultDateOffset)
    {
        if (save?.Data == null || offset + 8 > save.Data.Length)
            return false;

        ushort year = (ushort)newDate.Year;
        byte month = (byte)newDate.Month;
        byte day = (byte)newDate.Day;

        // Write primary date at offset
        BinaryPrimitives.WriteUInt16LittleEndian(save.Data.AsSpan(offset, 2), year);
        save.Data[offset + 2] = month;
        save.Data[offset + 3] = day;

        // Write secondary paired date at offset + 4
        BinaryPrimitives.WriteUInt16LittleEndian(save.Data.AsSpan(offset + 4, 2), year);
        save.Data[offset + 6] = month;
        save.Data[offset + 7] = day;

        // Synchronize save Description text (e.g. "16/8/2018" or "13/8/2018")
        UpdateDescriptionDate(save, newDate);

        return true;
    }

    /// <summary>
    /// Scans the decrypted data for the career date block signature.
    /// </summary>
    public static int FindDateOffset(byte[] data)
    {
        for (int i = 0; i < data.Length - 8; i++)
        {
            ushort y1 = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(i, 2));
            ushort y2 = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(i + 4, 2));

            if (y1 == y2 && y1 is >= 2018 and <= 2045)
            {
                byte m1 = data[i + 2];
                byte d1 = data[i + 3];
                byte m2 = data[i + 6];
                byte d2 = data[i + 7];

                if (m1 == m2 && m1 is >= 1 and <= 12 && d1 == d2 && d1 is >= 1 and <= 31)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    /// <summary>
    /// Updates the date pattern (d/M/yyyy) inside the Description text of the save.
    /// </summary>
    private static void UpdateDescriptionDate(PesSaveDescriptor save, DateTime newDate)
    {
        if (save.Description == null || save.Description.Length == 0)
            return;

        // PES save descriptions are ASCII / UTF-8
        string descStr = System.Text.Encoding.ASCII.GetString(save.Description);
        
        // Regex match d/M/yyyy or dd/MM/yyyy
        var match = Regex.Match(descStr, @"\b(\d{1,2})/(\d{1,2})/(\d{4})\b");
        if (match.Success)
        {
            string oldDateStr = match.Value;
            string newDateStr = $"{newDate.Day}/{newDate.Month}/{newDate.Year}";
            
            // Pad or format so length remains appropriate
            string updated = descStr.Substring(0, match.Index) + newDateStr + descStr.Substring(match.Index + oldDateStr.Length);
            
            byte[] newBytes = System.Text.Encoding.ASCII.GetBytes(updated);
            if (newBytes.Length <= save.Description.Length)
            {
                Array.Clear(save.Description, 0, save.Description.Length);
                Array.Copy(newBytes, save.Description, newBytes.Length);
            }
        }
    }
}
