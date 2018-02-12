using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Font
{
    public class FontReader
    {
        public void ReadFont(Stream stream)
        {
            OffsetTable offsetTable;
            List<TableHeader> unreadTables = new List<TableHeader>();

            // True-type fonts use big-endian.
            using (BinaryEndianAgnosticReader reader = new BinaryEndianAgnosticReader(stream, false))
            {
                offsetTable = new OffsetTable()
                {
                    MajorVersion = reader.ReadUInt16(),
                    MinorVersion = reader.ReadUInt16(),
                    NumTables = reader.ReadUInt16(),
                    SearchRange = reader.ReadUInt16(),
                    EntrySelector = reader.ReadUInt16(),
                    RangeShift = reader.ReadUInt16(),
                };

                for (int i = 0; i < offsetTable.NumTables; i++)
                {
                    unreadTables.Add(ReadTableHeader(reader));
                }
            }
        }

        private TableHeader ReadTableHeader(BinaryEndianAgnosticReader reader)
        {
            uint tagCode = reader.ReadUInt32();
            char[] tagChars = new char[4]
            {
                (char)((tagCode & 0xff000000) >> 24),
                (char)((tagCode & 0xff0000) >> 16),
                (char)((tagCode & 0xff00) >> 8),
                (char)(tagCode & 0xff)
            };

            return new TableHeader()
            {
                Tag = new string(tagChars),
                CheckSum = reader.ReadUInt32(),
                Offset = reader.ReadUInt32(),
                Length = reader.ReadUInt32(),
            };
        }
    }
}
