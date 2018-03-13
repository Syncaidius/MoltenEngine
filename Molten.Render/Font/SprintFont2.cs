using Molten.Collections;
using Molten.Font;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class SpriteFont2 : ISpriteFont
    {
        class CharData
        {
            // TODO store kerning/GPOS/GSUB offsets in pairs, on-demand.  

            public Rectangle Location;

            /// <summary>
            /// The glyph index of the character's glyph, within the base font.
            /// </summary>
            public int GlyphIndex;
        }

        FontFile _font;
        ThreadedQueue<char> _pendingCharacters;
        ITexture2D _texture;

        CharData[] _charData;

        public SpriteFont2(IRenderer renderer, FontFile font)
        {
            _font = font;
            _pendingCharacters = new ThreadedQueue<char>();
            _charData = new CharData[10];
        }

        public ITexture2D UnderlyingTexture => throw new NotImplementedException();

        public Rectangle GetCharRect(char c)
        {
            throw new NotImplementedException();
        }

        public Vector2F MeasureString(string text)
        {
            throw new NotImplementedException();
        }

        public Vector2F MeasureString(string text, int maxLength)
        {
            throw new NotImplementedException();
        }

        public Rectangle MeasureString(string text, int startIndex, int length)
        {
            throw new NotImplementedException();
        }

        public int NearestCharacter(string text, Vector2F localPosition)
        {
            throw new NotImplementedException();
        }

        public int FontSize => throw new NotImplementedException();

        public string FontName => throw new NotImplementedException();
    }
}
