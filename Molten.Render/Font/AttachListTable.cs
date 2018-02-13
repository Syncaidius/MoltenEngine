using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Font
{
    public class AttachListTable
    {
        CoverageTable _coverage;

        public void ReadTable(BinaryEndianAgnosticReader reader, Logger log, TableHeader header)
        {
            long attachStartOffset = reader.Position;
            ushort coverageOffset = reader.ReadUInt16();
            uint glyphCount = reader.ReadUInt16();
            _coverage = new CoverageTable();

            // TODO: Check this. Is previous font data being mis-read and causing this table to be empty?
                ushort[] attachPointOffsets = new ushort[glyphCount];
                for (int i = 0; i < glyphCount; i++)
                    attachPointOffsets[i] = reader.ReadUInt16();

                // Read the coverage table.
                reader.Position = attachStartOffset + coverageOffset;
                _coverage.ReadTable(reader, log, header);
        }
    }
}
