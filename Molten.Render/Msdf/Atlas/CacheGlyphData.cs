using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Typography.Contours;

namespace Typography.Rendering
{
    public class CacheGlyphData
    {
        public int borderX;
        public int borderY;
        public GlyphData img;
        public Rectangle area;
        public char character;
        public int codePoint;
        public GlyphMatrix2 glyphMatrix;

    }
}
