using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Horizontal Device Metrics (hdmx) table.<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/hdmx </summary>
    public class Cmap : FontTable
    {
        public ushort Version { get; internal set; }

        public SubTable[] SubTables { get; internal set; }

        Dictionary<int, ushort> _charIndexToGlyph = new Dictionary<int, ushort>();

        public ushort LookupIndex(int codepoint, int nextCodepoint = 0)
        {
            // MS Docs: Character codes that do not correspond to any glyph in the font should be mapped to glyph index 0.
            // See: https://www.microsoft.com/typography/OTSPEC/cmap.htm
            ushort result = 0;

            if (!_charIndexToGlyph.TryGetValue(codepoint, out result))
            {
                foreach (SubTable cmap in SubTables)
                {
                    ushort glyphID = cmap.CharToGlyphIndex(codepoint);

                    // MS Docs: When building a Unicode font for Windows, the platform ID should be 3 and the encoding ID should be 1.
                    // See: https://www.microsoft.com/typography/OTSPEC/cmap.htm
                    if (result == 0 || (glyphID != 0 && cmap.Platform == FontPlatform.Windows && cmap.Encoding == 1))
                        result = glyphID;
                }

                _charIndexToGlyph[codepoint] = result;
            }

            // If there is a second codepoint, we are asked whether this is an UVS sequence
            //  -> if true, return a glyph ID
            //  -> otherwise, return 0
            if (nextCodepoint > 0)
            {
                foreach (SubTable cmap in SubTables)
                {
                    ushort gylphID = cmap.CharPairToGlyphIndex(codepoint, result, nextCodepoint);
                    if (gylphID > 0)
                        return gylphID;
                }

                return 0;
            }

            return result;
        }

        public abstract class SubTable
        {
            public FontPlatform Platform { get; protected set; }

            public ushort Encoding { get; protected set; }

            public ushort Language { get; protected set; }

            public abstract ushort CharToGlyphIndex(int codepoint);

            public abstract ushort CharPairToGlyphIndex(int codepoint, ushort defaultGlyphIndex, int nextCodepoint);

            public SubTable(EncodingRecord record)
            {
                Platform = record.Platform;
                Encoding = record.Encoding;
            }

            internal abstract void Read(BinaryEndianAgnosticReader reader, Logger log, TableHeader header);
        }

        public class EncodingRecord
        {
            public FontPlatform Platform { get; internal set; }

            public ushort Encoding { get; internal set; }

            public uint Offset { get; internal set; }
        }

        internal class Parser : FontTableParser
        {
            public override string TableTag => "cmap";

            internal override FontTable Parse(BinaryEndianAgnosticReader reader, TableHeader header, Logger log, FontTableList dependencies)
            {
                ushort version = reader.ReadUInt16();
                ushort numRecords = reader.ReadUInt16();
                EncodingRecord[] records = new EncodingRecord[numRecords];

                // Read offsets and prepare records.
                for(int i = 0; i < numRecords; i++)
                {
                    records[i] = new EncodingRecord()
                    {
                        Platform = (FontPlatform)reader.ReadUInt16(),
                        Encoding = reader.ReadUInt16(),
                        Offset = reader.ReadUInt32(),
                    };
                }

                Cmap table = new Cmap()
                {
                    Version = version,
                    SubTables = new SubTable[numRecords],
                };

                // Populate records based on their format
                for(int i = 0; i < numRecords; i++)
                {
                    EncodingRecord record = records[i];
                    reader.Position = header.Offset + record.Offset;
                    ushort format = reader.ReadUInt16();

                    switch (format)
                    {
                        case 0: table.SubTables[i] = new CmapFormat0SubTable(record); break;
                        //case 2: ReadFormat2(reader, record); break; // Had no luck finding a font with format_2 cmap subtables. Need one for testing.
                        case 4: table.SubTables[i] = new CmapFormat4SubTable(record); break;
                        default:
                            log.WriteDebugLine($"[CMAP] Unsupported format for record {i}/{numRecords - 1}: Format {format}");
                            break;
                    }

                    table.SubTables[i]?.Read(reader, log, header);
                }

                return table;
            }
        }
    }    
}
