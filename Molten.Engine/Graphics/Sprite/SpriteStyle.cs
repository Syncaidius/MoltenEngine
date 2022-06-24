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
        public Color PrimaryColor;

        /// <summary>
        /// A secondary color. This may be a border or gradient color.
        /// </summary>
        public Color SecondaryColor;

        /// <summary>
        /// A thickness value. This may be for border, outline or line thickness.
        /// </summary>
        public float Thickness;

        public static readonly SpriteStyle Default = new SpriteStyle()
        {
            PrimaryColor = Color.White,
            SecondaryColor = Color.White,
            Thickness = 0
        };

        public SpriteStyle(Color color, float thickness = 0)
        {
            PrimaryColor = color;
            Thickness = thickness;
            SecondaryColor = color;
        }

        public SpriteStyle(Color color, Color borderColor, float thickness = 2)
        {
            PrimaryColor = color;
            SecondaryColor = borderColor;
            Thickness = thickness;
        }
    }
}
