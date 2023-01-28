using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EllipseStyle
    {
        public static readonly EllipseStyle Default = new EllipseStyle(Color.White);

        public Color FillColor;

        public Color BorderColor;

        public float BorderThickness;

        public EllipseStyle()
        {
            FillColor = Color.White;
            BorderColor = Color.White;
            BorderThickness = 0f;
        }

        public EllipseStyle(Color fillColor)
        {
            FillColor = fillColor;
            BorderColor = fillColor;
            BorderThickness = 0;
        }

        public EllipseStyle(Color fillColor, Color borderColor, float borderThickness)
        {
            FillColor = fillColor;
            BorderColor = borderColor;
            BorderThickness = borderThickness;
        }

        public static implicit operator EllipseStyle(Color color)
        {
            return new EllipseStyle()
            {
                FillColor = color,
                BorderColor = color,
                BorderThickness = 0f
            };
        }

        public static implicit operator Color(EllipseStyle style)
        {
            return style.FillColor;
        }
    }
}
