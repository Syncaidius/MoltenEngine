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

        /// <summary>
        /// 
        /// </summary>
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

            return font;
        }

        private void LoadTable(FontFile font, TableHeader header, List<TableHeader> toParse, Dictionary<string, TableHeader> toParseByTag)
        {
            FontTableParser parser = GetTableParser(header.Tag);
            if (parser != null)
            {
                _log.WriteDebugLine($"Found '{header.Tag}' parser -- table is {header.Length} bytes", _filename);
                Dictionary<string, FontTable> dependencies = new Dictionary<string, FontTable>();
                bool dependenciesValid = true;

                if (parser.Dependencies != null)
                {
                    // Attempt to load/retrieve dependency tables before continuing.
                    foreach (string depTag in parser.Dependencies)
                    {
                        FontTable dep = font[depTag];
                        if (dep == null)
                        {
                            if (toParseByTag.TryGetValue(depTag, out TableHeader depHeader))
                            {
                                _log.WriteDebugLine($"[{header.Tag}] Loading dependency '{depTag}'");
                                LoadTable(font, depHeader, toParse, toParseByTag);
                                dep = font[depTag];
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
                        dependencies.Add(depTag, dep);
                    }
                }

                if (dependenciesValid)
                {
                    // Move to the start of the table and parse it.
                    _reader.Position = header.Offset;
                    FontTable table = parser.Parse(_reader, header, _log);
                    table.Header = header;
                    font[header.Tag] = table;

                    long expectedEnd = header.Offset + header.Length;
                    long readerPos = _reader.Position;
                    long posDif = expectedEnd - readerPos;

                    if (expectedEnd != readerPos)
                        _log.WriteDebugLine($"Parsed font table '{header.Tag}' -- End pos: byte {readerPos} -- [MISMATCH] expected: {expectedEnd} -- dif: {posDif} bytes", _filename);
                    else
                        _log.WriteDebugLine($"Parsed font table '{header.Tag}' -- End pos: byte {readerPos} -- [CORRECT]", _filename);
                }
            }
            else
            {
                _log.WriteWarning($"Unsupported font table -- {header.ToString()}", _filename);
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
