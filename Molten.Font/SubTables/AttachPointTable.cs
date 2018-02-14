using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Font
{
    /// <summary>
    /// 
    /// </summary>
    public class AttachPointTable
    {
        /// <summary>Gets the byte offset of the <see cref="AttachPointTable"/> within it's parent <see cref="AttachListTable"/>.</summary>
        public ushort Offset { get; private set; }

        /// <summary>Gets an array of contour point indices for a glyph.</summary>
        public ushort[] ContourPointIndices { get; internal set; }

        /// <summary>Gets the parent <see cref="AttachListTable"/> of the current <see cref="AttachPointTable"/>.</summary>
        public AttachListTable Parent { get; private set; }

        /// <summary>Gets the ID of the glyph that these attach points belong to.</summary>
        public ushort GlyphID { get; internal set; }

        internal AttachPointTable(AttachListTable parent, ushort offset)
        {
            Parent = parent;
            Offset = offset;
        }
    }
}
