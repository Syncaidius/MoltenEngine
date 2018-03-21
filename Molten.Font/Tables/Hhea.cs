using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Font header table .<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/head </summary>
    [FontTableTag("hhea")]
    public class Hhea : FontTable
    {
        public ushort MajorVersion { get; private set; }

        public ushort MinorVersion { get; private set; }

        /// <summary>
        /// Gets the typographic ascent, in font design units. This is the Distance from baseline of highest ascender. <para/>
        /// See: (http://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6hhea.html
        /// </summary>
        public short Ascender { get; private set; }

        /// <summary>
        /// Gets the typographic descent, in font design units. This is the distance from baseline of lowest descender. <para/>
        /// See: (http://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6hhea.html
        /// </summary>
        public short Descender { get; private set; }

        /// <summary>
        /// Gets the typographic line gap, in font design units. Negative LineGap values are treated as zero in Windows 3.1, and in Mac OS System 6 and System 7.
        /// </summary>
        public short LineGap { get; private set; }

        /// <summary>
        /// Gets the maximum advance width value in 'hmtx' table, in font design units.
        /// </summary>
        public ushort AdvanceWidthMax { get; private set; }

        /// <summary>
        /// Gets the minimum left sidebearing value in 'hmtx' table, in font design units.
        /// </summary>
        public short MinLeftSideBearing { get; private set; }

        /// <summary>
        /// Gets the minimum right sidebearing value; Calculated as Min(aw - lsb - (xMax - xMin)).
        /// </summary>
        public short MinRightSideBearing { get; private set; }

        /// <summary>
        /// Gets the max X extent. Max(lsb + (xMax - xMin)).
        /// </summary>
        public short MaxExtentX { get; private set; }

        /// <summary>
        /// Gets the caret slope rise. Used to calculate the slope of the cursor (rise/run); 1 for vertical.
        /// </summary>
        public short CaretSlopeRise { get; private set; }

        /// <summary>
        /// Gets the caret slope run. 0 for vertical.
        /// </summary>
        public short CaretSlopeRun { get; private set; }

        /// <summary>
        /// Gets the amount by which a slanted highlight on a glyph needs to be shifted to produce the best appearance. Set to 0 for non-slanted fonts
        /// </summary>
        public short CaretOffset { get; private set; }

        /// <summary>
        /// Gets the 1st reserved value (usually 0).
        /// </summary>
        public short Reserved1 { get; private set; }

        /// <summary>
        /// Gets the 2nd reserved value (usually 0).
        /// </summary>
        public short Reserved2 { get; private set; }

        /// <summary>
        /// Gets the 3rd reserved value (usually 0).
        /// </summary>
        public short Reserved3 { get; private set; }

        /// <summary>
        /// Gets the 4th reserved value (usually 0).
        /// </summary>
        public short Reserved4 { get; private set; }

        /// <summary>
        /// Gets the metric data format. 0 for current format.
        /// </summary>
        public short MetricDataFormat { get; private set; }

        /// <summary>
        /// Gets the number of hMetric entries in 'hmtx' table
        /// </summary>
        public ushort NumberOfHMetrics { get; private set; }

        /// <summary>
        /// Gets the line spacing, in font design units. <para/>
        /// This is computed from the result of: linespace = Ascender - Descender + LineGap.
        /// </summary>
        public int LineSpace { get; private set; }

        internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
        {
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
            Ascender = reader.ReadInt16();
            Descender = reader.ReadInt16();
            LineGap = reader.ReadInt16();
            AdvanceWidthMax = reader.ReadUInt16();
            MinLeftSideBearing = reader.ReadInt16();
            MinRightSideBearing = reader.ReadInt16();
            MaxExtentX = reader.ReadInt16();
            CaretSlopeRise = reader.ReadInt16();
            CaretSlopeRun = reader.ReadInt16();
            CaretOffset = reader.ReadInt16();
            Reserved1 = reader.ReadInt16();
            Reserved2 = reader.ReadInt16();
            Reserved3 = reader.ReadInt16();
            Reserved4 = reader.ReadInt16();
            MetricDataFormat = reader.ReadInt16();
            NumberOfHMetrics = reader.ReadUInt16();

            LineSpace = Ascender - Descender + LineGap;
        }
    }

}
