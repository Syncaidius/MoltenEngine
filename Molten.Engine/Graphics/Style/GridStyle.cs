using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GridStyle
    {
        public Color CellColor;

        public Color LineColor;

        public Vector2F LineThickness;

        public GridStyle()
        {
            CellColor = Color.White;
            LineColor = Color.White * 0.8f;
            LineThickness = Vector2F.One;
        }

        public static implicit operator GridStyle(Color color)
        {
            return new GridStyle()
            {
                CellColor = color * 0.8f, // Make the cell slightly darker than the line color
                LineColor = color,
                LineThickness = Vector2F.One
            };
        }

        public static implicit operator Color(GridStyle style)
        {
            return style.CellColor;
        }
    }
}
