using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Font
{
    /// <summary>A table containing coverage indices. A glyph ID is used as an index for the <see cref="GlyphCoverageIndices"/> array.</summary>
    public class CoverageTable
    {
        public ushort Format { get; internal set; }

        /// <summary>Gets the starting ID within <see cref="GlyphCoverageIndices"/>.</summary>
        public ushort StartGlyphID { get; internal set; } = ushort.MaxValue;

        /// <summary>Gets a array containing the class ID's of each glyph. The ID of a glyph should be used as an index for the array.</summary>
        public ushort[] GlyphCoverageIndices => _coverageIndices;

        ushort[] _coverageIndices;

        internal void ReadTable(BinaryEndianAgnosticReader reader, Logger log, TableHeader header)
        {
            Format = reader.ReadUInt16();

            if (Format == 1) // CoverageFormat1 - list
            {
                ushort glyphCount = reader.ReadUInt16();
                _coverageIndices = new ushort[glyphCount];
                for (ushort i = 0; i < glyphCount; i++)
                {
                    int glyphID = reader.ReadUInt16();
                    GlyphCoverageIndices[glyphID] = i;
                }
            }
            else if (Format == 2) // CoverageFormat2 - ranges
            {
                ushort rangeCount = reader.ReadUInt16();
                for (ushort i = 0; i < rangeCount; i++)
                {
                    ushort glyphStartID = reader.ReadUInt16();
                    ushort glyphEndID = reader.ReadUInt16();
                    ushort startCoverageIndex = reader.ReadUInt16();

                    StartGlyphID = Math.Min(glyphStartID, StartGlyphID);
                    if (GlyphCoverageIndices == null || glyphEndID >= GlyphCoverageIndices.Length)
                        Array.Resize(ref _coverageIndices, glyphEndID);

                    for (ushort g = glyphStartID; g < glyphEndID; g++)
                        _coverageIndices[g] = (ushort)(startCoverageIndex + g - glyphStartID);
                }
            }
            else
            {
                log.WriteWarning($"Unsupported coverage sub-table in font '{header.Tag}' table");
            }
        }
    }
}
