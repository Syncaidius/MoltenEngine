using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class LigatureCaretListTable
    {
        /// <summary>Gets an array containing AttachPoint tables ordered by coverage index, which hold contour point indices.</summary>
        public LigatureGlyphTable[] GlyphTables { get; private set; }

        internal LigatureCaretListTable(BinaryEndianAgnosticReader reader, Logger log, long startPos)
        {
            ushort coverageOffset = reader.ReadUInt16();
            uint ligGlyphCount = reader.ReadUInt16();
            GlyphTables = new LigatureGlyphTable[ligGlyphCount];

            // prepare attach point tables with their respective offsets.
            for (int i = 0; i < ligGlyphCount; i++)
                GlyphTables[i] = new LigatureGlyphTable(this, reader.ReadUInt16());

            // Read the coverage table.
            CoverageTable coverage = new CoverageTable(reader, log, startPos + coverageOffset);

            // Populate attach points in each AttachPointTable.
            for (int i = 0; i < ligGlyphCount; i++)
            {
                LigatureGlyphTable pt = GlyphTables[i];
                reader.Position = startPos + pt.Offset;

                ushort caretCount = reader.ReadUInt16();
                pt.CaretValues = new CaretValue[caretCount];
                for (int p = 0; p < caretCount; p++)
                {
                    CaretValueFormat format = (CaretValueFormat)reader.ReadUInt16();
                    int cv = 0;
                    DeviceVariationIndexTable dvt = null;

                    if (format == CaretValueFormat.One_Coord)
                        cv = reader.ReadInt16(); // signed int16 here (just to make it obvious!)
                    else if (format == CaretValueFormat.Two_ContourPointIndex)
                        cv = reader.ReadUInt16(); // unsigned.
                    else if (format == CaretValueFormat.Three_CoordWithDVarTable)
                    {
                        cv = reader.ReadInt16(); // signed.
                        dvt = new DeviceVariationIndexTable(reader, log);
                    }

                    CaretValue val = new CaretValue(format, cv, dvt);
                    pt.GlyphID = coverage.Glyphs[p];
                }
            }
        }
    }
}
