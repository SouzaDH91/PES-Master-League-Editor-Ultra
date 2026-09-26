using System.Buffers.Binary;
using System.Text;
using Pes2019MlEditor.Core.IO;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class TraceAllTables
{
    private readonly ITestOutputHelper _output;
    public TraceAllTables(ITestOutputHelper output) => _output = output;

    [Fact]
    public void TraceTables()
    {
        string path = @"C:\Users\Diego\Documents\HANO4U\PRO EVOLUTION SOCCER 2019\292733975847239680\save\ML00000000";
        var desc = SaveFileManager.LoadSave(path);

        int currentOffset = 0;
        int tableIndex = 0;

        while (currentOffset < desc.Data.Length - 80)
        {
            var span = desc.Data.AsSpan(currentOffset);
            int magic = BinaryPrimitives.ReadInt32LittleEndian(span[0..4]);
            int headerSize = BinaryPrimitives.ReadInt32LittleEndian(span[4..8]);
            int totalTableSize = BinaryPrimitives.ReadInt32LittleEndian(span[8..12]);
            int recordCount = BinaryPrimitives.ReadInt32LittleEndian(span[12..16]);
            int recordSize = BinaryPrimitives.ReadInt32LittleEndian(span[16..20]);

            if (headerSize != 80 && headerSize != 64 && headerSize != 96)
            {
                // Not standard header or end of structured tables
                break;
            }

            _output.WriteLine($"Table #{tableIndex} at 0x{currentOffset:X8}: Magic={magic}, HeaderSize={headerSize}, TotalSize={totalTableSize}, Records={recordCount}, RecSize={recordSize}");

            // Look at first 16 bytes of first record
            if (recordCount > 0 && recordSize > 0 && currentOffset + headerSize + 16 <= desc.Data.Length)
            {
                string firstRecHex = Convert.ToHexString(desc.Data.AsSpan(currentOffset + headerSize, Math.Min(32, recordSize)));
                string firstRecAscii = Encoding.ASCII.GetString(desc.Data.AsSpan(currentOffset + headerSize, Math.Min(32, recordSize))).Replace('\0', '.');
                _output.WriteLine($"   First record preview: {firstRecHex} | {firstRecAscii}");
            }

            int advance = headerSize + (recordCount * recordSize);
            if (advance <= headerSize) break;
            currentOffset += advance;
            tableIndex++;
        }

        _output.WriteLine($"Stopped structured scan at 0x{currentOffset:X8} (Remaining: {desc.Data.Length - currentOffset} bytes)");
    }
}
