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
        struct CharData
        {
            public ushort GlyphIndex;

            public bool Initialized;

            public CharData(ushort gIndex)
            {
                GlyphIndex = gIndex;
                Initialized = true;
            }
        }

        FontGlyphBinding[] _bindings;
        CharData[] _charData;


        public FontBinding(FontManager manager, FontFile font)
        {
            Manager = manager;
            Font = font;

            if (Font.GlyphCount > 0)
            {
                _bindings = new FontGlyphBinding[Font.GlyphCount];
            }
            else
            {
                _bindings = new FontGlyphBinding[1];
                _bindings[0] = new FontGlyphBinding(Manager, font, '\0');
            }

            _charData = new CharData[char.MaxValue]; // TODO Optimize this - 65536 Char data entries is overkill.       
        }

        public FontFile Font { get; }

        internal FontManager Manager { get; }
    }
}
