using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class FontReader : IDisposable
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

        Stream _stream;
        Logger _log;
        string _filename;
        bool _leaveOpen;
        BinaryEndianAgnosticReader _reader;

        /// <summary>Creates a new instance of <see cref="FontReader"/>.</summary>
        /// <param name="stream">A stream containing from which to read font data.</param>
        /// <param name="log">A logger.</param>
        /// <param name="filename">An optional filename or label to improve log/debug messages.</param>
        /// <param name="leaveOpen">If true, the underlying stream will not be closed or disposed when the <see cref="FontReader"/> is disposed.</param>
        public FontReader(Stream stream, Logger log, string filename = null, bool leaveOpen = false)
        {
            _stream = stream;
            _log = log;
            _filename = filename;
            _leaveOpen = leaveOpen;
            _reader = new BinaryEndianAgnosticReader(_stream, false, leaveOpen);
        }

        /// <summary>Parses a .TTF or .OTF font file and returns a new <see cref="FontFile"/> instance containing detailed information about a font.</summary>
        /// <param name="stream"></param>
        /// <param name="log"></param>
        /// <param name="filename">Optional. Provided simply to improve log errors and message.</param>
        public FontFile ReadFont()
        {
            OffsetTable offsetTable;
            FontFile font = new FontFile();

            List<TableHeader> toParse = new List<TableHeader>();
            Dictionary<string, TableHeader> toParseByTag = new Dictionary<string, TableHeader>();

            // True-type fonts use big-endian.
            offsetTable = new OffsetTable()
            {
                MajorVersion = _reader.ReadUInt16(),
                MinorVersion = _reader.ReadUInt16(),
                NumTables = _reader.ReadUInt16(),
                SearchRange = _reader.ReadUInt16(),
                EntrySelector = _reader.ReadUInt16(),
                RangeShift = _reader.ReadUInt16(),
            };

            // Read table header entries and calculate where the end of the font file should be.
            long expectedEndPos = _stream.Position;
            for (int i = 0; i < offsetTable.NumTables; i++)
            {
                TableHeader header = ReadTableHeader(_reader);
                expectedEndPos += header.Length;

                toParse.Add(header);
                toParseByTag.Add(header.Tag, header);
            }

            // Now parse the tables.
            while(toParse.Count > 0)
            {
                TableHeader header = toParse[toParse.Count - 1];
                LoadTable(font, header, toParse, toParseByTag);
            }

            // Spit out warnings for unsupported font tables
            foreach(TableHeader header in font.Tables.UnsupportedTables)
                _log.WriteWarning($"Unsupported table -- {header.ToString()}", _filename);

            /* Jump to the end of the font file data within the stream.
             * Due to table depedency checks, we cannot guarantee the last table to be read is at the end of the font data, so this
             * avoids messing up the stream in a situation where multiple files/fonts/data-sets are held in the same file.*/
            _stream.Position = expectedEndPos;

            font.Build();
            return font;
        }

        private void LoadTable(FontFile font, TableHeader header, List<TableHeader> toParse, Dictionary<string, TableHeader> toParseByTag)
        {
            FontTableParser parser = GetTableParser(header.Tag);
            if (parser != null)
            {
                _log.WriteDebugLine($"Supported table '{header.Tag}' found ({header.Length} bytes)", _filename);
                FontTableList dependencies = new FontTableList();
                bool dependenciesValid = true;

                if (parser.Dependencies != null && parser.Dependencies.Length > 0)
                {
                    _log.WriteDebugLine($"[{header.Tag}] Dependencies: {string.Join(",", parser.Dependencies)}");

                    // Attempt to load/retrieve dependency tables before continuing.
                    foreach (string depTag in parser.Dependencies)
                    {
                        FontTable dep = font.Tables.Get(depTag);
                        if (dep == null)
                        {
                            if (toParseByTag.TryGetValue(depTag, out TableHeader depHeader))
                            {
                                _log.WriteDebugLine($"[{header.Tag}] Attempting to load missing dependency '{depTag}'");
                                LoadTable(font, depHeader, toParse, toParseByTag);
                                dep = font.Tables.Get(depTag);
                                if(dep == null)
                                {
                                    _log.WriteDebugLine($"[{header.Tag}] Dependency '{depTag}' failed to load correctly. Unable to load table.");
                                    dependenciesValid = false;
                                    break;
                                }
                            }
                            else
                            {
                                _log.WriteDebugLine($"[{header.Tag}] Missing dependency '{depTag}'. Unable to load table.");
                                dependenciesValid = false;
                                break;
                            }
                        }
                        else
                        {

                        }

                        _log.WriteDebugLine($"[{header.Tag}] Dependency '{depTag}' found");
                        dependencies.Add(dep);
                    }
                }

                if (dependenciesValid)
                {
                    // Move to the start of the table and parse it.
                    _reader.Position = header.Offset;
                    FontTable table = parser.Parse(_reader, header, _log, dependencies);
                    table.Header = header;
                    font.Tables.Add(table);

                    long expectedEnd = header.Offset + header.Length;
                    long readerPos = _reader.Position;
                    long posDif = expectedEnd - readerPos;

                    if (expectedEnd != readerPos)
                        _log.WriteDebugLine($"Parsed table '{header.Tag}' -- [MISMATCH] End pos (byte): {readerPos}. Expected: {expectedEnd} -- dif: {(posDif > 0 ? "+" : "-")}{posDif} bytes", _filename);
                    else
                        _log.WriteDebugLine($"Parsed table '{header.Tag}' -- [PASS]", _filename);
                }
            }
            else
            {
                font.Tables.AddUnsupported(header);
            }

            // Successful or not, we're done with the current table.
            toParse.Remove(header);
            toParseByTag.Remove(header.Tag);
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
                Tag = new string(tagChars).Trim(),
                CheckSum = reader.ReadUInt32(),
                Offset = reader.ReadUInt32(),
                Length = reader.ReadUInt32(),
            };
        }

        public void Dispose()
        {
            _reader.Close();
            _stream.Dispose();
        }
    }
}
