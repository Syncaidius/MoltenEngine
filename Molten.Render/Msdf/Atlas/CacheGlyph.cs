using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Typography.Contours;

namespace Typography.Rendering
{
    public class CacheGlyph
    {
        public int borderX;
        public int borderY;
        public GlyphImage img;
        public Rectangle area;
        public char character;
        public int codePoint;
        public GlyphMatrix2 glyphMatrix;

    }
}
