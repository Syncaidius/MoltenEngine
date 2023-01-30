namespace Molten.Graphics
{
    public partial class SpriteBatcher
    {
        public void DrawShapeOutline(Shape shape, Color color, float thickness, uint surfaceSlice = 0)
        {
            DrawShapeOutline(shape, color, color, thickness, surfaceSlice);
        }

        public void DrawShapeOutline(Shape shape, Color edgeColor, Color holeColor, float thickness, uint surfaceSlice = 0)
        {
            foreach (Shape.Contour c in shape.Contours)
            {
                Color col = c.GetWinding() < 1 ? edgeColor : holeColor;

                foreach (Shape.Edge e in c.Edges)
                    DrawEdge(e, col, thickness, surfaceSlice);
            }
        }

        public void DrawShapeOutline(Shape shape, Vector2F position, ref LineStyle style, uint surfaceSlice = 0)
        {
            LineStyle edgeStyle = style;

            foreach (Shape.Contour c in shape.Contours)
            {
                edgeStyle.Color1 = edgeStyle.Color2 = c.GetWinding() < 1 ? style.Color1 : style.Color2;

                foreach (Shape.Edge e in c.Edges)
                    DrawEdge(e, ref style, surfaceSlice);
            }
        }
    }
}
