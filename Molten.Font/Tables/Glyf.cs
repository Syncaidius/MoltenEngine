using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Glyf data table (glyf).<para/>
    /// This table contains information that describes the glyphs in the font in the TrueType outline format. 
    /// Information regarding the rasterizer (scaler) refers to the TrueType rasterizer. See: https://docs.microsoft.com/en-us/typography/opentype/spec/head </summary>
    public class Glyf : FontTable
    {        

        internal class Parser : FontTableParser
        {
            static string[] _dependencies = new string[] { "loca", "maxp" };

            public override string TableTag => "glyf";

            public override string[] Dependencies => _dependencies;

            internal override FontTable Parse(BinaryEndianAgnosticReader reader, TableHeader header, Logger log, FontTableList dependencies)
            {
                Loca loca = dependencies.Get<Loca>();
                Maxp maxp = dependencies.Get<Maxp>();

                /* https://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6glyf.html
                 * Glyph descrion:
                 * Type 	Name 	Description
                 * SHORT 	numberOfContours 	If the number of contours is >= zero, this is a single glyph. If it's negative, this is a composite glyph.
                 * SHORT 	xMin 	Minimum x for coordinate data.
                 * SHORT 	yMin 	Minimum y for coordinate data.
                 * SHORT 	xMax 	Maximum x for coordinate data.
                 * SHORT 	yMax 	Maximum y for coordinate data.
                 */

                ushort numGlyphs = maxp.NumGlyphs;
                GlyphData[] data = new GlyphData[numGlyphs];
                List<ushort> compositeGlyphs = new List<ushort>();

                for (int i = 0; i < numGlyphs; i++)
                {
                    uint offset = loca.Offsets[i];
                    uint length = loca.Offsets[i + 1] - offset;

                    if(length == 0)
                    {
                        if (i == 0)
                        {
                            // Character 0 has no data, make some.
                            data[i] = new GlyphData()
                            {
                                // TODO add data for a simple box character.
                            };
                        }
                        else
                        {
                            // Use glyph 0 if the current glyph has no data.
                            data[i] = data[0];
                        }
                    }
                    else
                    {
                        // If the number of contours is >= zero, this is a single glyph. If it's negative, this is a composite glyph.
                        short numContours = reader.ReadInt16();
                        if(numContours >= 0)
                        {

                        }
                        else
                        {

                        }
                    }
                }

                // TODO resolve composite glyph data here.

                Glyf table = new Glyf();                

                return table;
            }
        }
    }

}
