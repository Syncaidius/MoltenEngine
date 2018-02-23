using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Font header table .<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/head </summary>
    public class Head : FontTable
    {
        public const uint EXPECTED_MAGIC_NUMBER = 0x5F0F3CF5;

        public ushort MajorVersion { get; private set; }

        public ushort MinorVersion { get; private set; }

        public ushort FontRevisionMajor { get; private set; }

        public ushort FontRevisionMinor { get; private set; }

        public uint ChecksumAdjustment { get; private set; }

        public uint MagicNumber { get; private set; }

        public FontHeadFlags Flags { get; private set; }

        public ushort UnitsPerEm { get; private set; }

        public DateTime Created { get; private set; }

        public DateTime Modified { get; private set; }

        public short MinX { get; private set; }

        public short MinY { get; private set; }

        public short MaxX { get; private set; }

        public short MaxY { get; private set; }

        public MacStyleFlags MacStyle { get; private set; }

        public ushort LowestRecPPEM { get; private set; }

        public FontDirectionHint DirectionHint { get; private set; }

        /// <summary>Gets the expected format of the index-to-location (loca) table, if present.</summary>
        public FontLocaFormat LocaFormat { get; private set; }

        public short GlyphDataFormat { get; private set; }

        internal class Parser : FontTableParser
        {
            public override string TableTag => "head";

            internal override FontTable Parse(BinaryEndianAgnosticReader reader, TableHeader header, Logger log, FontTableList dependencies)
            {
                Head table = new Head()
                {
                    MajorVersion = reader.ReadUInt16(),
                    MinorVersion = reader.ReadUInt16(),
                    FontRevisionMajor = reader.ReadUInt16(),
                    FontRevisionMinor = reader.ReadUInt16(),
                    ChecksumAdjustment = reader.ReadUInt32(),
                    MagicNumber = reader.ReadUInt32(),
                    Flags = (FontHeadFlags)reader.ReadUInt16(),
                    UnitsPerEm = reader.ReadUInt16(),
                    Created = FontUtil.FromLongDate(reader.ReadInt64()),
                    Modified = FontUtil.FromLongDate(reader.ReadInt64()),
                    MinX = reader.ReadInt16(),
                    MinY = reader.ReadInt16(),
                    MaxX = reader.ReadInt16(),
                    MaxY = reader.ReadInt16(),
                    MacStyle = (MacStyleFlags)reader.ReadUInt16(),
                    LowestRecPPEM = reader.ReadUInt16(),
                    DirectionHint = (FontDirectionHint) reader.ReadInt16(),
                    LocaFormat = (FontLocaFormat)reader.ReadInt16(),
                    GlyphDataFormat = reader.ReadInt16(),
                };

                if (table.MagicNumber != EXPECTED_MAGIC_NUMBER)
                    log.WriteDebugLine($"[head] Invalid magic number detected: {table.MagicNumber} -- Expected: {EXPECTED_MAGIC_NUMBER}");

                return table;
            }
        }
    }

}
