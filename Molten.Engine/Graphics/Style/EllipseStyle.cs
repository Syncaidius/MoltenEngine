using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EllipseStyle
    {
        public Color FillColor;

        public Color BorderColor;

        public float BorderThickness;

        public EllipseStyle()
        {
            FillColor = Color.White;
            BorderColor = Color.White;
            BorderThickness = 1.0f;
        }

        public static implicit operator EllipseStyle(Color color)
        {
            return new EllipseStyle()
            {
                FillColor = color,
                BorderColor = color,
                BorderThickness = 2f
            };
        }

        public static implicit operator Color(EllipseStyle style)
        {
            return style.FillColor;
        }
    }
}
