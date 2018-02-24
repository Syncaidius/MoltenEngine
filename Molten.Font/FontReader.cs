using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Molten.Font
{
    public class FontReader : IDisposable
    {
        class TableEntry
        {
            public Type Type;
            public string[] Dependencies;
        }

        static Dictionary<string, TableEntry> _tableTypes;

        static FontReader()
        {
            _tableTypes = new Dictionary<string, TableEntry>();
            IEnumerable<Type> tableTypes = ReflectionHelper.FindTypesWithAttribute<FontTableTagAttribute>(typeof(FontTableTagAttribute).Assembly);
            foreach(Type t in tableTypes)
            {
                if (t.IsAbstract)
                    continue;

                FontTableTagAttribute att = t.GetCustomAttribute<FontTableTagAttribute>();
                _tableTypes.Add(att.Tag, new TableEntry()
                {
                    Type = t,
                    Dependencies = att.Dependencies,
                });
            }
        }

        static FontTable GetTable(TableHeader header)
        {
            if (_tableTypes.TryGetValue(header.Tag, out TableEntry entry))
            {
                FontTable table = Activator.CreateInstance(entry.Type) as FontTable;
                table.Header = header;
                table.Dependencies = entry.Dependencies.Clone() as string[];
                return table;
            }

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
        /// <param name="buildFontWhenDone">If true, the font will be built - by calling <see cref="FontFile.Build"/> - when font data has been completely read.</param>
        /// <param name="ignoredTables">One or more tags (of font tables) to be ignored, if any. Ignored tables will not be parsed/loaded.</param>
        public FontFile ReadFont(bool buildFontWhenDone = true, params string[] ignoredTables)
        {
            OffsetTable offsetTable;
            FontTableList tables = new FontTableList();

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
                bool ignored = false;

                // Check if table is ignored.
                if(ignoredTables != null)
                {
                    for(int j = 0; j < ignoredTables.Length; j++)
                    {
                        if(ignoredTables[j] == header.Tag)
                        {
                            _log.WriteDebugLine($"Ignoring table '{header.Tag}' ({header.Length} bytes)", _filename);
                            ignored = true;
                            break;
                        }
                    }
                }

                if (!ignored) {
                    toParse.Add(header);
                    toParseByTag.Add(header.Tag, header);
                }
            }

            // Now parse the tables.
            while(toParse.Count > 0)
            {
                TableHeader header = toParse[toParse.Count - 1];
                LoadTable(tables, header, toParse, toParseByTag);
            }

            // Spit out warnings for unsupported font tables
            foreach(TableHeader header in tables.UnsupportedTables)
                _log.WriteWarning($"Unsupported table -- {header.ToString()}", _filename);

            /* Jump to the end of the font file data within the stream.
             * Due to table depedency checks, we cannot guarantee the last table to be read is at the end of the font data, so this
             * avoids messing up the stream in a situation where multiple files/fonts/data-sets are held in the same file.*/
            _stream.Position = expectedEndPos;

            FontFile font = new FontFile(tables);
            if (buildFontWhenDone)
                font.Build();

            return font;
        }

        private void LoadTable(FontTableList tables, TableHeader header, List<TableHeader> toParse, Dictionary<string, TableHeader> toParseByTag)
        {
            FontTable table = GetTable(header);
            if (table != null)
            {
                _log.WriteDebugLine($"Supported table '{header.Tag}' found ({header.Length} bytes)", _filename);
                FontTableList dependencies = new FontTableList();
                bool dependenciesValid = true;

                if (table.Dependencies != null && table.Dependencies.Length > 0)
                {
                    _log.WriteDebugLine($"[{header.Tag}] Dependencies: {string.Join(",", table.Dependencies)}");

                    // Attempt to load/retrieve dependency tables before continuing.
                    foreach (string depTag in table.Dependencies)
                    {
                        FontTable dep = tables.Get(depTag);
                        if (dep == null)
                        {
                            if (toParseByTag.TryGetValue(depTag, out TableHeader depHeader))
                            {
                                _log.WriteDebugLine($"[{header.Tag}] Attempting to load missing dependency '{depTag}'");
                                LoadTable(tables, depHeader, toParse, toParseByTag);
                                dep = tables.Get(depTag);
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

                        _log.WriteDebugLine($"[{header.Tag}] Dependency '{depTag}' found");
                        dependencies.Add(dep);
                    }
                }

                if (dependenciesValid)
                {
                    // Move to the start of the table and parse it.
                    _reader.Position = header.Offset;
                    table.Read(_reader, header, _log, dependencies);
                    tables.Add(table);

                    long expectedEnd = header.Offset + header.Length;
                    long readerPos = _reader.Position;
                    long posDif = readerPos - expectedEnd;

                    if (expectedEnd != readerPos)
                        _log.WriteDebugLine($"Parsed table '{header.Tag}' -- [MISMATCH] End pos (byte): {readerPos}. Expected: {expectedEnd} -- dif: {(posDif > 0 ? "+" : "")}{posDif} bytes", _filename);
                    else
                        _log.WriteDebugLine($"Parsed table '{header.Tag}' -- [PASS]", _filename);
                }
            }
            else
            {
                tables.AddUnsupported(header);
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
