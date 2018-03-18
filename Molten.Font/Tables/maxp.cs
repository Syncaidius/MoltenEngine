using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Maximum profile table (maxp).<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/maxp </summary>
    [FontTableTag("maxp")]
    public class Maxp : FontTable
    {
        public ushort MajorVersion { get; private set; }

        public ushort MinorVersion { get; private set; }

        /// <summary>
        /// The number of glyphs in the font.
        /// </summary>
        public ushort NumGlyphs { get; private set; }

        /// <summary>
        /// Maximum points in a non-composite glyph.
        /// </summary>
        public ushort MaxPoints { get; private set; }

        /// <summary>
        /// Maximum contours in a non-composite glyph.
        /// </summary>
        public ushort MaxContours { get; private set; }

        /// <summary>
        /// Maximum points in a composite glyph.
        /// </summary>
        public ushort MaxCompositePoints { get; private set; }

        /// <summary>
        /// Maximum contours in a composite glyph.
        /// </summary>
        public ushort MaxCompositeContours { get; private set; }

        /// <summary>
        /// 1 if instructions do not use the twilight zone (Z0), or 2 if instructions do use Z0; should be set to 2 in most cases.
        /// </summary>
        public ushort MaxZones { get; private set; }

        /// <summary>
        /// Maximum points used in Z0.
        /// </summary>
        public ushort MaxTwilightPoints { get; private set; }

        /// <summary>
        /// Number of Storage Area locations.
        /// </summary>
        public ushort MaxStorage { get; private set; }

        /// <summary>
        /// Number of FDEFs, equal to the highest function number + 1.
        /// </summary>
        public ushort MaxFunctionDefs { get; private set; }

        /// <summary>
        /// Number of IDEFs.
        /// </summary>
        public ushort MaxInstructionDefs { get; private set; }

        /// <summary>
        /// Maximum stack depth. (This includes Font and CVT Programs, as well as the instructions for each glyph.)
        /// </summary>
        public ushort MaxStackElements { get; private set; }

        /// <summary>
        /// Maximum byte count for glyph instructions.
        /// </summary>
        public ushort MaxSizeOfInstructions { get; private set; }

        /// <summary>
        /// Maximum number of components referenced at "top level" for any composite glyph.
        /// </summary>
        public ushort MaxComponentElements { get; private set; }

        /// <summary>
        /// Maximum levels of recursion; 1 for simple components.
        /// </summary>
        public ushort MaxComponentDepth { get; private set; }

        internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
        {
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
            NumGlyphs = reader.ReadUInt16();
            MaxPoints = reader.ReadUInt16();
            MaxContours = reader.ReadUInt16();
            MaxCompositePoints = reader.ReadUInt16();
            MaxCompositeContours = reader.ReadUInt16();
            MaxZones = reader.ReadUInt16();
            MaxTwilightPoints = reader.ReadUInt16();
            MaxStorage = reader.ReadUInt16();
            MaxFunctionDefs = reader.ReadUInt16();
            MaxInstructionDefs = reader.ReadUInt16();
            MaxStackElements = reader.ReadUInt16();
            MaxSizeOfInstructions = reader.ReadUInt16();
            MaxComponentElements = reader.ReadUInt16();
            MaxComponentDepth = reader.ReadUInt16();
        }
    }
}
