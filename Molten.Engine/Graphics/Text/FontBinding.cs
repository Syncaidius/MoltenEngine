using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Font;

namespace Molten.Graphics
{
    internal class FontBinding
    {
        internal FontGlyphBinding[] Glyphs { get; }

        internal CharData[] Data { get; }

        public FontBinding(FontManager manager, FontFile font)
        {
            Manager = manager;
            File = font;

            if (File.GlyphCount > 0)
            {
                Glyphs = new FontGlyphBinding[File.GlyphCount];
            }
            else
            {
                Glyphs = new FontGlyphBinding[1];
                Glyphs[0] = new FontGlyphBinding(this)
                {
                    AdvanceWidth = 1,
                    AdvanceHeight = 1,
                    Location = new Rectangle(0, 0, 1, 1),
                    Page = 0,
                    PWidth = 1,
                    PHeight = 1,
                    YOffset = 0
                };
            }

            Data = new CharData[char.MaxValue]; // TODO Optimize this - 65536 Char data entries is overkill.       
        }

        public FontGlyphBinding GetCharacter(char c)
        {
            if (!Data[c].Initialized)
                Manager.AddCharacter(this, c, true);

            return Glyphs[Data[c].GlyphIndex] ?? Glyphs[Data[FontManager.PLACEHOLDER_CHAR].GlyphIndex];
        }

        public FontFile File { get; }

        internal FontManager Manager { get; }
    }
}
