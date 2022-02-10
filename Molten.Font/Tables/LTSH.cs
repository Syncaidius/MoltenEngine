using Molten.IO;

namespace Molten.Font
{
    /// <summary>LTSH - Linear Threshold Table. <para/>
    /// The LTSH table relates to OpenType™ fonts containing TrueType outlines. <para/>
    /// There are noticeable improvements to fonts on the screen when instructions are carefully applied to the sidebearings. The gain in readability is offset by the necessity for the OS to grid fit the glyphs in order to find the actual advance width for the glyphs (since instructions may be moving the sidebearing points). <para/>
    /// The TrueType outline format already has two mechanisms to side step the speed issues: the 'hdmx' table, where precomputed advance widths may be saved for selected ppem sizes, and the 'vdmx' table, where precomputed vertical advance widths may be saved for selected ppem sizes. The 'LTSH' table (Linear ThreSHold) is a second, complementary method.
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/ltsh </summary>
    [FontTableTag("LTSH")]
    public class LTSH : FontTable
    {
        public ushort Version { get; private set; }

        /// <summary>
        /// Gets the vertical pel height at which the glyph can be assumed to scale linearly. Stored on a per-glyph basis. <para/>
        /// Note that glyphs which do not have instructions on their sidebearings should have yPels = 1; i.e., always scales linearly.
        /// </summary>
        public byte[] YPels { get; private set; }

        internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
        {
            Version = reader.ReadUInt16();
            ushort numGlyphs = reader.ReadUInt16();
            YPels = reader.ReadBytes(numGlyphs);
        }
    }
}
