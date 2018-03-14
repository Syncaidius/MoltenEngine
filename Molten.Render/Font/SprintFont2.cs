using Molten.Collections;
using Molten.Font;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class SpriteFont2
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
        int _fontSize;

        CharData[] _charData;
        int _lockerval;

        public SpriteFont2(IRenderer renderer, FontFile font)
        {
            _font = font;
            _pendingCharacters = new ThreadedQueue<char>();
            _charData = new CharData[10];
        }

        public ITexture2D UnderlyingTexture => throw new NotImplementedException();

        public Rectangle GetCharRect(char c)
        {
            SpinWait spin = new SpinWait();
            while (Interlocked.Exchange(ref _lockerval, 1) != 0)
                spin.SpinOnce();

            if (_charData.Length <= c)
                Array.Resize(ref _charData, Math.Min(char.MaxValue, c + 100));

            Rectangle rect;
            if(_charData[c] == null)
            {
                rect = new Rectangle(10, 10, 10, 10); // TODO: get bounds using sheet packer.
                _charData[c] = new CharData()
                {
                    GlyphIndex = _font.GetGlyphIndex(c),
                    Location = rect,
                };

                // Queue the character for generation.
                _pendingCharacters.Enqueue(c);
            }
            else
            {
                rect = _charData[c].Location;
            }

            Interlocked.Exchange(ref _lockerval, 0);
            return rect;
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

        /// <summary>
        /// The font size, in font points (e.g. 12pt, 16pt, 18pt, etc).
        /// </summary>
        public int FontSize => _fontSize;

        /// <summary>
        /// Gets the underlying font used to generate the sprite-font.
        /// </summary>
        public FontFile Font => _font;
    }
}
