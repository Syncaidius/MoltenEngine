using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RectStyle
    {
        public static readonly RectStyle Default = new RectStyle();

        public Color FillColor;

        public Color BorderColor;

        public RectBorderThickness BorderThickness;

        public RectStyle()
        {
            FillColor = Color.White;
            BorderColor = Color.White;
            BorderThickness = new RectBorderThickness(0);
        }

        public RectStyle(Color color)
        {
            FillColor = color;
            BorderColor = color;
            BorderThickness = new RectBorderThickness(0);
        }

        public RectStyle(Color fillColor, Color borderColor, float borderThickness)
        {
            FillColor = fillColor;
            BorderColor = borderColor;
            BorderThickness = new RectBorderThickness(borderThickness);
        }

        public RectStyle(Color fillColor, Color borderColor, ref RectBorderThickness borderThickness)
        {
            FillColor = fillColor;
            BorderColor = borderColor;
            BorderThickness = borderThickness;
        }

        public RectStyle(Color fillColor, Color borderColor, RectBorderThickness borderThickness) : 
            this(fillColor, borderColor, ref borderThickness)
        { }

        public static implicit operator RectStyle(Color color)
        {
            return new RectStyle()
            {
                FillColor = color,
                BorderColor = color,
                BorderThickness = new RectBorderThickness(0f)
            };
        }

        public static implicit operator Color(RectStyle style)
        {
            return style.FillColor;
        }
    }
}
