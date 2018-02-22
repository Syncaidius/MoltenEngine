using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class AttachListTable
    {
        /// <summary>Gets an array containing AttachPoint tables ordered by coverage index, which hold contour point indices.</summary>
        public AttachPointTable[] PointTables { get; private set; }

        internal AttachListTable(BinaryEndianAgnosticReader reader, Logger log, long startPos)
        {
            reader.Position = startPos;
            ushort coverageOffset = reader.ReadUInt16();
            uint glyphCount = reader.ReadUInt16();
            PointTables = new AttachPointTable[glyphCount];

            // prepare attach point tables with their respective offsets.
            for (int i = 0; i < glyphCount; i++)
                PointTables[i] = new AttachPointTable(this, reader.ReadUInt16());

            // Read the coverage table.
            CoverageTable coverage = new CoverageTable(reader, log, startPos + coverageOffset);

            // Populate attach points in each AttachPointTable.
            for (int i = 0; i < glyphCount; i++)
            {
                AttachPointTable pt = PointTables[i];
                reader.Position = startPos + pt.Offset;

                ushort pointCount = reader.ReadUInt16();
                pt.ContourPointIndices = new ushort[pointCount];
                for (int p = 0; p < pointCount; p++)
                {
                    pt.ContourPointIndices[p] = reader.ReadUInt16();
                    pt.GlyphID = coverage.Glyphs[p];
                }
            }
        }
    }
}
