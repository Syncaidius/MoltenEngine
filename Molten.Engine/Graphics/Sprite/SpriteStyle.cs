using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public struct SpriteStyle
    {
        /// <summary>
        /// The main color.
        /// </summary>
        public Color4 Color;

        /// <summary>
        /// A thickness value. This may be for border, outline or line thickness.
        /// </summary>
        public float Thickness;

        /// <summary>
        /// A secondary color. This may be a border or gradient color.
        /// </summary>
        public Color4 Color2;

        public static readonly SpriteStyle Default = new SpriteStyle()
        {
            Color = Color4.White,
            Color2 = Color4.White,
            Thickness = 0
        };

        public SpriteStyle(Color color, float thickness = 2)
        {
            Color = color;
            Thickness = thickness;
            Color2 = color;
        }

        public SpriteStyle(Color color, Color borderColor, float thickness = 2)
        {
            Color = color;
            Color2 = borderColor;
            Thickness = thickness;
        }
    }
}
