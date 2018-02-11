using Molten;
using System;
using Typography.Contours;
using RectangleF = Typography.Contours.RectangleF;
using Rectangle = Typography.Contours.Rectangle;

namespace Typography.Rendering
{  
    public struct GlyphMatrix2
    {
        public short unit_per_em;
        public short ascender;
        public short descender;
        public short height;
        public int advanceX;
        public int advanceY;
        public int bboxXmin;
        public int bboxXmax;
        public int bboxYmin;
        public int bboxYmax;
        public int img_width;
        public int img_height;
        public int img_horiBearingX;
        public int img_horiBearingY;
        public int img_horiAdvance;
        public int img_vertBearingX;
        public int img_vertBearingY;
        public int img_vertAdvance;
        public int bitmap_left;
        public int bitmap_top;
        //public IntPtr bitmap;
        //public IntPtr outline;
    }
}