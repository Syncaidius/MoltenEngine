using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SpriteLineStyle
    {
        public Color Color1;

        public Color Color2;

        public float Thickness;

        public float Sharpness;

        public SpriteLineStyle()
        {
            Color1 = Color.White;
            Color2 = Color.White;
            Thickness = 1.0f;
            Sharpness = 1.0f;
        }

        public static implicit operator SpriteLineStyle(Color color)
        {
            return new SpriteLineStyle()
            {
                Color1 = color,
                Color2 = color,
                Thickness = 2f,
                Sharpness = 1f
            };
        }

        public static implicit operator Color(SpriteLineStyle style)
        {
            return style.Color1;
        }
    }
}
