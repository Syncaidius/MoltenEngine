using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract partial class SpriteBatcher
    {
        public void DrawShapeOutline(Shape shape, Color color, float thickness)
        {
            DrawShapeOutline(shape, color, color, thickness);
        }

        public void DrawShapeOutline(Shape shape, Color edgeColor, Color holeColor, float thickness)
        {
            foreach (Shape.Contour c in shape.Contours)
            {
                Color col = c.GetWinding() < 1 ? edgeColor : holeColor;

                foreach (Shape.Edge e in c.Edges)
                    DrawEdge(e, col, thickness);
            }
        }

        public void DrawShapeOutline(Shape shape, Vector2F position)
        {
            foreach (Shape.Contour c in shape.Contours)
            {
                Color col = c.GetWinding() < 1 ? _style.PrimaryColor : _style.SecondaryColor;

                foreach (Shape.Edge e in c.Edges)
                    DrawEdge(e, col, _style.Thickness);
            }
        }
    }
}
