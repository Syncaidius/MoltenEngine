using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RoundedRectStyle
    {
        public static readonly RectStyle Default = new RectStyle();

        public Color FillColor;

        public Color BorderColor;

        public float BorderThickness;

        public CornerInfo CornerRadius;

        public RoundedRectStyle()
        {
            FillColor = Color.White;
            BorderColor = Color.White;
            BorderThickness = 1f;
            CornerRadius = new CornerInfo(10);
        }

        public RoundedRectStyle(Color color)
        {
            FillColor = color;
            BorderColor = color;
            BorderThickness = 0f;
            CornerRadius = new CornerInfo(10);
        }

        public RoundedRectStyle(Color fillColor, Color borderColor, float borderThickness)
        {
            FillColor = fillColor;
            BorderColor = borderColor;
            BorderThickness = borderThickness;
            CornerRadius = new CornerInfo(10);
        }

        public RectStyle ToRectStyle()
        {
            return new RectStyle()
            {
                FillColor = FillColor,
                BorderColor = BorderColor,
                BorderThickness = new RectBorderThickness(BorderThickness)
            };
        }

        public static implicit operator RoundedRectStyle(Color color)
        {
            return new RoundedRectStyle(color, color, 0);
        }

        public static implicit operator RoundedRectStyle(RectStyle style)
        {
            return style.FillColor;
        }
    }
}
