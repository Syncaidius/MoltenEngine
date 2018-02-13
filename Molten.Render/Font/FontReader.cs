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
        static Dictionary<string, FontTableParser> _tableParsers;

        static FontReader()
        {
            _tableParsers = new Dictionary<string, FontTableParser>();
            IEnumerable<Type> parserTypes = ReflectionHelper.FindType<FontTableParser>(typeof(FontTableParser).Assembly);
            foreach(Type t in parserTypes)
            {
                FontTableParser parser = Activator.CreateInstance(t) as FontTableParser;
                _tableParsers.Add(parser.TableTag, parser);
            }
        }

        static FontTableParser GetTableParser(string tableTag)
        {
            if (_tableParsers.TryGetValue(tableTag, out FontTableParser parser))
                return parser;
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="log"></param>
        /// <param name="filename">Optional. Provided simply to improve log errors and message.</param>
        public void ReadFont(Stream stream, Logger log, string filename = null)
        {
            OffsetTable offsetTable;
            List<TableHeader> headers = new List<TableHeader>();
            Dictionary<string, FontTable> completedTables = new Dictionary<string, FontTable>();

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

                // Read table header entries.
                for (int i = 0; i < offsetTable.NumTables; i++)
                    headers.Add(ReadTableHeader(reader));

                // TODO Check for presence of REQUIRED tables before proceeding here.
                // TODO See: https://www.microsoft.com/typography/otspec/otff.htm
                // TODO Also consider TTF tables and CFF tables for OpenType data. Certain tables require or refer to other table types.
                //      Example: TTF fonts require glyf and loca tables. OpenType fonts require a CFF or CFF2 table.

                // Now parse the tables.
                foreach (TableHeader th in headers)
                {
                    FontTableParser parser = GetTableParser(th.Tag);
                    if (parser != null)
                    {
                        // Move to the start of the table and parse it.
                        reader.Position = th.Offset;
                        FontTable table = parser.Parse(reader, th, log);
                        table.Header = th;
                        completedTables.Add(th.Tag, table);
                        log.WriteDebugLine($"Parsed font table '{th.Tag}'", filename);
                    }
                    else
                    {
                        log.WriteWarning($"Unsupported font table '{th.Tag}'", filename);
                    }
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
