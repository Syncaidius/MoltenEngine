using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Font
{
    public class AttachListTable
    {
        /// <summary>Gets an array containing AttachPoint tables ordered by coverage index, which hold contour point indices.</summary>
        public AttachPointTable[] AttachPointTables { get; private set; }

        public void ReadTable(BinaryEndianAgnosticReader reader, Logger log, TableHeader header)
        {
            long attachStartOffset = reader.Position;
            ushort coverageOffset = reader.ReadUInt16();
            uint glyphCount = reader.ReadUInt16();
            CoverageTable coverage = new CoverageTable();
            AttachPointTables = new AttachPointTable[glyphCount];

            // prepare attach point tables with their respective offsets.
            for (int i = 0; i < glyphCount; i++)
                AttachPointTables[i] = new AttachPointTable(this, reader.ReadUInt16());

            // Read the coverage table.
            reader.Position = attachStartOffset + coverageOffset;
            coverage.ReadTable(reader, log, header);

            // Populate attach points in each AttachPointTable.
            for (int i = 0; i < glyphCount; i++)
            {
                AttachPointTable pt = AttachPointTables[i];
                reader.Position = attachStartOffset + pt.Offset;

                ushort pointCount = reader.ReadUInt16();
                pt.ContourPointIndices = new ushort[pointCount];
                for (int p = 0; p < pointCount; p++)
                    pt.ContourPointIndices[p] = reader.ReadUInt16();
            }
        }
    }
}
