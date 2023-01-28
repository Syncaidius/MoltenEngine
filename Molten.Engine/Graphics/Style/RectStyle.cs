using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RectStyle
    {
        public static readonly RectStyle Default = new RectStyle();

        public Color FillColor;

        public Color BorderColor;

        public Thickness BorderThickness;

        public RectStyle()
        {
            FillColor = Color.White;
            BorderColor = Color.White;
            BorderThickness = new Thickness(0);
        }

        public RectStyle(Color color)
        {
            FillColor = color;
            BorderColor = color;
            BorderThickness = new Thickness(0);
        }

        public RectStyle(Color fillColor, Color borderColor, float borderThickness)
        {
            FillColor = fillColor;
            BorderColor = borderColor;
            BorderThickness = new Thickness(borderThickness);
        }

        public RectStyle(Color fillColor, Color borderColor, ref Thickness borderThickness)
        {
            FillColor = fillColor;
            BorderColor = borderColor;
            BorderThickness = borderThickness;
        }

        public RectStyle(Color fillColor, Color borderColor, Thickness borderThickness) : 
            this(fillColor, borderColor, ref borderThickness)
        { }

        public static implicit operator RectStyle(Color color)
        {
            return new RectStyle()
            {
                FillColor = color,
                BorderColor = color,
                BorderThickness = new Thickness(0f)
            };
        }

        public static implicit operator Color(RectStyle style)
        {
            return style.FillColor;
        }
    }
}
