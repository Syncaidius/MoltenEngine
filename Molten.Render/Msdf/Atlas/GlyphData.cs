using Molten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RectangleF = Typography.Contours.RectangleF;
using Rectangle = Typography.Contours.Rectangle;

namespace Typography.Rendering
{
    public class GlyphData
    {
        Color[] PixelBuffer;

        public GlyphData(int w, int h)
        {
            Width = w;
            Height = h;
        }
        public RectangleF OriginalGlyphBounds { get; set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public bool IsBigEndian { get; private set; }

        public int BorderXY { get; set; }

        public Color[] GetImageBuffer()
        {
            return PixelBuffer;
        }

        public void SetImageBuffer(Color[] pixelBuffer, bool isBigEndian)
        {
            this.PixelBuffer = pixelBuffer;
            IsBigEndian = isBigEndian;
        }
        /// <summary>
        /// texture offset X from original glyph
        /// </summary>
        public double TextureOffsetX { get; set; }
        /// <summary>
        /// texture offset Y from original glyph 
        /// </summary>
        public double TextureOffsetY { get; set; }
    }
}
