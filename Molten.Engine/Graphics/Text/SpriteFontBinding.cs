using Molten.Font;

namespace Molten.Graphics
{
    internal class SpriteFontBinding
    {
        internal SpriteFontGlyphBinding[] Glyphs { get; }

        internal CharData[] Data { get; }

        public SpriteFontBinding(SpriteFontManager manager, FontFile font)
        {
            Manager = manager;
            File = font;

            if (File.GlyphCount > 0)
            {
                Glyphs = new SpriteFontGlyphBinding[File.GlyphCount];
            }
            else
            {
                Glyphs = new SpriteFontGlyphBinding[1];
                Glyphs[0] = new SpriteFontGlyphBinding(this)
                {
                    AdvanceWidth = 1,
                    AdvanceHeight = 1,
                    Location = new Rectangle(0, 0, 1, 1),
                    PageID = 0,
                    PWidth = 1,
                    PHeight = 1,
                    YOffset = 0
                };
            }

            Data = new CharData[char.MaxValue]; // TODO Optimize this - 65536 Char data entries is overkill.       

            Manager.AddCharacter(this, SpriteFontManager.PLACEHOLDER_CHAR, false);

            /*Rectangle pcRect = Glyphs[Data[' '].GlyphIndex].Location;
            pcRect.Width *= Manager.TabSize;
            AddCharacter('\t', false, pcRect);*/
            Manager.AddCharacter(this, '\t', false);
        }

        public SpriteFontGlyphBinding GetCharacter(char c)
        {
            if (!Data[c].Initialized)
                Manager.AddCharacter(this, c, true);

            return Glyphs[Data[c].GlyphIndex] ?? Glyphs[Data[SpriteFontManager.PLACEHOLDER_CHAR].GlyphIndex];
        }

        public FontFile File { get; }

        internal SpriteFontManager Manager { get; }
    }
}
