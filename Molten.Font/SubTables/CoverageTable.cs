using System;
using Molten.IO;

namespace Molten.Font
{
    /// <summary>TTF/OTF font coverage table. See: https://www.microsoft.com/typography/otspec/chapter2.htm#coverageTbl</summary>
    public class CoverageTable : FontSubTable
    {
        public ushort Format { get; internal set; }

        /// <summary>Gets an array containing the IDs of each glyph referenced by the coverage table.</summary>
        public ushort[] Glyphs => _glyphIDs;

        ushort[] _glyphIDs;

        internal CoverageTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
            base(reader, log, parent, offset)
        {
            Format = reader.ReadUInt16();

            if (Format == 1) // CoverageFormat1 - list
            {
                ushort glyphCount = reader.ReadUInt16();
                _glyphIDs = reader.ReadArray<ushort>(glyphCount);
            }
            else if (Format == 2) // CoverageFormat2 - ranges
            {
                ushort rangeCount = reader.ReadUInt16();
                for (ushort i = 0; i < rangeCount; i++)
                {
                    ushort glyphStartID = reader.ReadUInt16();
                    ushort glyphEndID = reader.ReadUInt16();
                    ushort coverageIndex = reader.ReadUInt16();

                    if (_glyphIDs == null || glyphEndID >= _glyphIDs.Length)
                        Array.Resize(ref _glyphIDs, glyphEndID);

                    for (ushort g = glyphStartID; g < glyphEndID; g++)
                        _glyphIDs[coverageIndex++] = g;
                }
            }
            else
            {
                log.Warning($"Unsupported coverage format: {Format}");
            }
        }

        public override string ToString()
        {
            return $"{this.GetType().Name} -- Format: {Format} -- Count: {(_glyphIDs != null ? _glyphIDs.Length.ToString() : "Invalid")}";
        }
    }
}
