using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>
    /// 
    /// </summary>
    public class AttachPointTable : FontSubTable
    {
        /// <summary>Gets an array of contour point indices for a glyph.</summary>
        public ushort[] ContourPointIndices { get; internal set; }

        internal override void Read(EnhancedBinaryReader reader, FontReaderContext context, FontTable parent)
        {
            ushort pointCount = reader.ReadUInt16();
            ContourPointIndices = reader.ReadArray<ushort>(pointCount);
        }
    }
}
