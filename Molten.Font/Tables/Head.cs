using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Font header table (head).<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/head </summary>
    [FontTableTag("head")]
    public class Head : FontTable
    {
        /// <summary>
        /// The expected magic number value of a valid <see cref="Head"/> table.
        /// </summary>
        public const uint EXPECTED_MAGIC_NUMBER = 0x5F0F3CF5;

        public ushort MajorVersion { get; private set; }

        public ushort MinorVersion { get; private set; }

        public ushort FontRevisionMajor { get; private set; }

        public ushort FontRevisionMinor { get; private set; }

        public uint ChecksumAdjustment { get; private set; }

        /// <summary>
        /// Gets the magic number for the <see cref="Head"/> table.
        /// </summary>
        public uint MagicNumber { get; private set; }

        /// <summary>
        /// Gets the font head flags which provide basic information about the rules and design of the font.
        /// </summary>
        public FontHeadFlags Flags { get; private set; }

        /// <summary>
        /// Gets the design units-per-em of the font.
        /// </summary>
        public ushort DesignUnitsPerEm { get; private set; }

        /// <summary>
        /// Gets the date the font was created. <para/>
        /// Note: This is not the date that the font file itself was created.
        /// </summary>
        public DateTime Created { get; private set; }

        /// <summary>
        /// Gets the date the font was last modified. <para/>
        /// Note: This is not the date that the font file itself was modified.
        /// </summary>
        public DateTime Modified { get; private set; }

        /// <summary>
        /// Gets the minimum X bounds for all glyph bounding boxes.
        /// </summary>
        public short MinX { get; private set; }

        /// <summary>
        /// Gets the minimum Y bounds for all glyph bounding boxes.
        /// </summary>
        public short MinY { get; private set; }

        /// <summary>
        /// Gets the maximum X bounds for all glyph bounding boxes.
        /// </summary>
        public short MaxX { get; private set; }

        /// <summary>
        /// Gets the maximum Y bounds for all glyph bounding boxes.
        /// </summary>
        public short MaxY { get; private set; }

        /// <summary>
        /// Gets macintosh-style flags. 
        /// </summary>
        public MacStyleFlags MacStyle { get; private set; }

        /// <summary>
        /// Gets the smallest readable size (pixels-per-em) in pixels.
        /// </summary>
        public ushort LowestRecPPEM { get; private set; }

        /// <summary>
        /// Gets the direction hint of the font.
        /// </summary>
        public FontDirectionHint DirectionHint { get; private set; }

        /// <summary>Gets the expected format of the index-to-location (loca) table, if present.</summary>
        public FontLocaFormat LocaFormat { get; private set; }

        /// <summary>
        /// Gets the glyph data format.
        /// </summary>
        public short GlyphDataFormat { get; private set; }

        internal override void Read(BinaryEndianAgnosticReader reader, TableHeader header, Logger log, FontTableList dependencies)
        {
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
            FontRevisionMajor = reader.ReadUInt16();
            FontRevisionMinor = reader.ReadUInt16();
            ChecksumAdjustment = reader.ReadUInt32();
            MagicNumber = reader.ReadUInt32();
            Flags = (FontHeadFlags)reader.ReadUInt16();
            DesignUnitsPerEm = reader.ReadUInt16();
            Created = FontUtil.FromLongDate(reader.ReadInt64());
            Modified = FontUtil.FromLongDate(reader.ReadInt64());
            MinX = reader.ReadInt16();
            MinY = reader.ReadInt16();
            MaxX = reader.ReadInt16();
            MaxY = reader.ReadInt16();
            MacStyle = (MacStyleFlags)reader.ReadUInt16();
            LowestRecPPEM = reader.ReadUInt16();
            DirectionHint = (FontDirectionHint)reader.ReadInt16();
            LocaFormat = (FontLocaFormat)reader.ReadInt16();
            GlyphDataFormat = reader.ReadInt16();

            if (MagicNumber != EXPECTED_MAGIC_NUMBER)
                log.WriteDebugLine($"[head] Invalid magic number detected: {MagicNumber} -- Expected: {EXPECTED_MAGIC_NUMBER}");
        }
    }

}
