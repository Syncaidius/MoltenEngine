using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Horizontal Device Metrics (hdmx) table.<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/hdmx </summary>
    [FontTableTag("hdmx", "maxp")]
    public class Hdmx : FontTable
    {
        public ushort Version { get; internal set; }

        public DeviceRecord[] Records { get; internal set; }

        /// <summary>
        /// Gets an array of <see cref="LongHorMetric"/> instances containing paired advance width and left side bearing values for each glyph. Records are indexed by glyph ID.
        /// </summary>
        public DeviceRecord[] Metrics { get; private set; }

        internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
        {
            Maxp tableMaxp = dependencies.Get<Maxp>();
            ushort numGlyphs = tableMaxp.NumGlyphs;

            ushort version = reader.ReadUInt16();
            short numRecords = reader.ReadInt16();
            int sizeDeviceRecord = reader.ReadInt32(); // Record size after padding.

            // MS docs: Each DeviceRecord is padded with 0's to make it 32-bit (4 bytes) aligned.
            int actualRecordSize = 2 + numGlyphs;
            int padding = actualRecordSize % 4;

            Records = new DeviceRecord[numRecords];
            for (int i = 0; i < numRecords; i++)
            {
                Records[i] = new DeviceRecord()
                {
                    PixelSize = reader.ReadByte(),
                    MaxWidth = reader.ReadByte(),
                    Widths = reader.ReadBytes(numGlyphs),
                };

                reader.Position += padding;
            }

            // NOTE: For some reason this table sometimes fall short of header.Length despite having all the correct data present.
            // If it fell short, jump to the expected table end position for debugging purposes.
            long expectedEnd = header.ReadOffset + header.Length;
            if (reader.Position < expectedEnd)
                reader.Position = expectedEnd;
        }
    }

    public class DeviceRecord
    {
        /// <summary>
        /// Gets the pixel size for following width, as ppem.
        /// </summary>
        public byte PixelSize { get; internal set; }

        /// <summary>
        /// Gets the maximum width.
        /// </summary>
        public byte MaxWidth { get; internal set; }

        /// <summary>
        /// Gets an array of widths. The length should match <see cref="Maxp.NumGlyphs"/>.<para/>
        /// Each Width value is the width of the particular glyph, in pixels, at the pixels per em (ppem) size listed at the start of the DeviceRecord.
        /// </summary>
        public byte[] Widths { get; internal set; }
    }
}
