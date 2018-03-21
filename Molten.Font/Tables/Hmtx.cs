using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Horizontal metrics (hmtx) table.<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/hmtx </summary>
    [FontTableTag("hmtx", "hhea", "maxp")]
    public class Hmtx : FontTable
    {
        LongHorMetric[] _metrics;

        /// <summary>
        /// Gets an array of <see cref="LongHorMetric"/> instances containing paired advance width and left side bearing values for each glyph. Records are indexed by glyph ID.
        /// </summary>
        public LongHorMetric[] Metrics => _metrics;

        internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
        {
            Hhea tableHhea = dependencies.Get<Hhea>();
            Maxp tableMaxp = dependencies.Get<Maxp>();

            ushort numHMetrics = tableHhea.NumberOfHMetrics;
            ushort numGlyphs = tableMaxp.NumGlyphs;
            _metrics = new LongHorMetric[numGlyphs];

            for (int i = 0; i < numHMetrics; i++)
            {
                Metrics[i] = new LongHorMetric()
                {
                    AdvanceWidth = reader.ReadUInt16(),
                    LeftSideBearing = reader.ReadInt16(),
                };
            }

            /* The table uses a longHorMetric record to give the advance width and left side bearing of a glyph. 
             * Records are indexed by glyph ID. As an optimization, the number of records can be less than the number of glyphs, 
             * in which case the advance width value of the last record applies to all remaining glyph IDs.
             * This can be useful in monospaced fonts, or in fonts that have a large number of glyphs with the same advance width (provided the glyphs are ordered appropriately). 
             * The number of longHorMetric records is determined by the numberOfHMetrics field in the 'hhea' table.
             */

            // If numHMetrics is less than the total number of glyphs, then that array is followed by an array for the left side bearing values of the remaining glyphs.
            // Here we'll fill in the remaining LongHorMetric using the rules described above.
            // Eats a tad more memory, but we're going for speed not tiny memory footprints!
            if (numHMetrics < numGlyphs)
            {
                int remaining = numGlyphs - numHMetrics;
                ushort lastAdvWidth = Metrics[numHMetrics - 1].AdvanceWidth;

                for (int i = numHMetrics; i < numGlyphs; i++)
                {
                    Metrics[i] = new LongHorMetric()
                    {
                        AdvanceWidth = lastAdvWidth,
                        LeftSideBearing = reader.ReadInt16(),
                    };
                }
            }
        }

        public ushort GetAdvanceWidth(int index)
        {
            return _metrics[index].AdvanceWidth;
        }
        public short GetLeftSideBearing(int index)
        {
            return _metrics[index].LeftSideBearing;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LongHorMetric
    {
        /// <summary>
        /// Advance width, in font design units.
        /// </summary>
        public ushort AdvanceWidth { get; internal set; }

        /// <summary>
        /// Glyph left side bearing (lsb), in font design units.
        /// </summary>
        public short LeftSideBearing { get; internal set; }
    }
}
