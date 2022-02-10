using Molten.IO;

namespace Molten.Font
{
    /// <summary>
    /// 
    /// </summary>
    public class AttachPointTable : FontSubTable
    {
        /// <summary>Gets an array of contour point indices for a glyph.</summary>
        public ushort[] ContourPointIndices { get; internal set; }

        internal AttachPointTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, ushort offset) :
            base(reader, log, parent, offset)
        {
            ushort pointCount = reader.ReadUInt16();
            ContourPointIndices = reader.ReadArray<ushort>(pointCount);
        }
    }
}
