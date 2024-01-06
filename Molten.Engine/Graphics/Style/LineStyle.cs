using System.Runtime.InteropServices;

namespace Molten.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LineStyle
{
    public static readonly LineStyle Default = new LineStyle(Color.White);

    public Color Color1;

    public Color Color2;

    public float Thickness;

    public float Sharpness;

    public LineStyle()
    {
        Color1 = Color.White;
        Color2 = Color.White;
        Thickness = 1.0f;
        Sharpness = 1.0f;
    }

    public LineStyle(Color color, float thickness = 1.0f, float sharpness = 1.0f) : 
        this(color, color, thickness, sharpness) { }

    public LineStyle(Color color1, Color color2, float thickness = 1.0f, float sharpness = 1.0f)
    {
        Color1 = color1;
        Color2 = color2;
        Thickness = thickness;
        Sharpness = sharpness;
    }

    public static implicit operator LineStyle(Color color)
    {
        return new LineStyle()
        {
            Color1 = color,
            Color2 = color,
            Thickness = 2f,
            Sharpness = 1f
        };
    }

    public static implicit operator Color(LineStyle style)
    {
        return style.Color1;
    }
}
