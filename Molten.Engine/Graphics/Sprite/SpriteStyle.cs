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
        public Color Color;

        /// <summary>
        /// A secondary color. This may be a border or gradient color.
        /// </summary>
        public Color Color2;

        /// <summary>
        /// A thickness value. This may be for border, outline or line thickness.
        /// </summary>
        public float Thickness;

        public static readonly SpriteStyle Default = new SpriteStyle()
        {
            Color = Color.White,
            Color2 = Color.White,
            Thickness = 0
        };

        public SpriteStyle(Color color, float thickness = 0)
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
