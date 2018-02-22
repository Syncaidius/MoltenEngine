using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Index-to-location table.<para/>
    /// <para>The indexToLoc table stores the offsets to the locations of the glyphs in the font, relative to the beginning of the glyphData table. In order to compute the length of the last glyph element, there is an extra entry after the last valid index.</para>
    /// <para>By definition, index zero points to the "missing character," which is the character that appears if a character is not found in the font. The missing character is commonly represented by a blank box or a space. If the font does not contain an outline for the missing character, then the first and second offsets should have the same value. This also applies to any other characters without an outline, such as the space character. If a glyph has no outline, then loca[n] = loca [n+1]. In the particular case of the last glyph(s), loca[n] will be equal the length of the glyph data ('glyf') table. The offsets must be in ascending order with loca[n] less-or-equal-to loca[n+1].</para>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/loca </summary>
    public class Loca : FontTable
    {
        public uint[] Offsets { get; internal set; }

        internal class Parser : FontTableParser
        {
            static string[] _dependencies = new string[] { "head", "maxp" };

            public override string TableTag => "loca";

            public override string[] Dependencies => _dependencies;

            internal override FontTable Parse(BinaryEndianAgnosticReader reader, TableHeader header, Logger log, FontTableList dependencies)
            {
                /* From Apple docs:
                 * The size of entries in the 'loca' table must be appropriate for the value of the indexToLocFormat field of the 'head' table. 
                 * The number of entries must be the same as the numGlyphs field of the 'maxp' table.
                 */
                Head head = dependencies.Get<Head>();
                Maxp maxp = dependencies.Get<Maxp>();

                // From MS + Apple Docs: In order to compute the length of the last glyph element, there is an extra entry after the last valid index.
                int numGlyphs = maxp.NumGlyphs + 1;

                uint[] offsets;
                switch (head.LocaFormat)
                {
                    // 16-bit loca format stores the original values divided by two (val / 2).
                    // To reverse, simply multiply them by 2 here.
                    case FontLocaFormat.UnsignedInt16:
                        offsets = new uint[numGlyphs];
                        for (int i = 0; i < numGlyphs; i++)
                            offsets[i] = reader.ReadUInt16() * 2U;
                        break;

                    default:
                    case FontLocaFormat.UnsignedInt32:
                        offsets = reader.ReadArrayUInt32(numGlyphs);
                        break;
                }

                return new Loca() { Offsets = offsets };
            }
        }
    }

}
