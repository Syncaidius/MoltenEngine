using Molten.IO;

namespace Molten.Font
{
    public class LigatureGlyphTable : FontSubTable
    {
        /// <summary>Gets an array of <see cref="CaretValue"/> for a glyph.</summary>
        public CaretValue[] CaretValues { get; internal set; }

        /// <summary>Gets the ID of the glyph that the <see cref="CaretValues"/> correspond to.</summary>
        public ushort GlyphID { get; internal set; }

        internal LigatureGlyphTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, CoverageTable coverage) :
            base(reader, log, parent, offset)
        {
            ushort caretCount = reader.ReadUInt16();
            CaretValues = new CaretValue[caretCount];
            for (int p = 0; p < caretCount; p++)
            {
                CaretValueFormat format = (CaretValueFormat)reader.ReadUInt16();
                int cv = 0;
                DeviceVariationIndexTable dvt = null;

                if (format == CaretValueFormat.DesignUnits)
                    cv = reader.ReadInt16(); // signed int16 here (just to make it obvious!)
                else if (format == CaretValueFormat.ContourPointIndex)
                    cv = reader.ReadUInt16(); // unsigned.
                else if (format == CaretValueFormat.DesignUnitsPlusDVarTable)
                {
                    cv = reader.ReadInt16(); // signed.
                    ushort dvtOffset = reader.ReadUInt16();
                    dvt = new DeviceVariationIndexTable(reader, log, this, dvtOffset);
                }

                CaretValue val = new CaretValue(format, cv, dvt);
                GlyphID = coverage.Glyphs[p];
            }
        }
    }
}
