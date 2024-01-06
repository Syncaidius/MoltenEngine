using Molten.Shapes;

namespace Molten.Graphics;

public partial class SpriteBatcher
{
    public void DrawShapeOutline(Shape shape, Color color, float thickness, uint surfaceSlice = 0)
    {
        DrawShapeOutline(shape, color, color, thickness, surfaceSlice);
    }

    public void DrawShapeOutline(Shape shape, Color edgeColor, Color holeColor, float thickness, uint surfaceSlice = 0)
    {
        Contour c;
        for(int i = 0; i < shape.Contours.Count; i++)
        {
            c = shape.Contours[i];
            Color col = c.GetWinding() < 1 ? edgeColor : holeColor;

            foreach (Edge e in c.Edges)
                DrawEdge(e, col, thickness, surfaceSlice);
        }
    }

    public void DrawShapeOutline(Shape shape, Vector2F position, ref LineStyle style, uint surfaceSlice = 0)
    {
        LineStyle edgeStyle = style;

        Contour c;
        for (int i = 0; i < shape.Contours.Count; i++)
        {
            c = shape.Contours[i];
            edgeStyle.Color1 = edgeStyle.Color2 = c.GetWinding() < 1 ? style.Color1 : style.Color2;


            for (int j = 0; j < c.Edges.Count; j++)
                DrawEdge(c.Edges[j], ref style, surfaceSlice);
        }
    }
}
