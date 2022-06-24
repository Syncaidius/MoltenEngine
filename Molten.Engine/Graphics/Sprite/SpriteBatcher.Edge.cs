using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract partial class SpriteBatcher
    {
        /// <summary>Draws connecting lines between each of the provided points.</summary>
        /// <param name="edge">The edge to draw.</param>
        /// <param name="color">The color of the lines</param>
        /// <param name="thickness">The thickness of the line in pixels.</param>
        public void DrawEdge(Shape.LinearEdge edge, Color color, float thickness)
        {
            DrawLine((Vector2F)edge.P[0], (Vector2F)edge.P[1], color, thickness);
        }

        /// <summary>Draws connecting lines between each of the provided points.</summary>
        /// <param name="edge">The edge to draw.</param>
        public void DrawEdge(Shape.LinearEdge edge)
        {
            DrawLine((Vector2F)edge.P[0], (Vector2F)edge.P[1]);
        }

        /// <summary>Draws .</summary>
        /// <param name="edge">The edge to draw.</param>
        public void DrawEdge(Shape.Edge edge, Color color, float thickness, float resolution = 16)
        {
            double increment = 1.0 / resolution;
            Vector2F p1;
            Vector2F p2 = (Vector2F)edge.Point(0);

            for (int i = 0; i < resolution; i++)
            {
                p1 = p2;
                p2 = (Vector2F)edge.Point(increment * i);

                DrawLine(p1, p2, color, thickness);
            }

            // Draw final line to reach edge.End;
            DrawLine(p2, (Vector2F)edge.End, color, thickness);
        }

        /// <summary>Draws .</summary>
        /// <param name="edge">The edge to draw.</param>
        public void DrawEdge(Shape.Edge edge, float resolution = 16)
        {
            double increment = 1.0 / resolution;
            Vector2F p1;
            Vector2F p2 = (Vector2F)edge.Point(0);

            for (int i = 0; i < resolution; i++)
            {
                p1 = p2;
                p2 = (Vector2F)edge.Point(increment * i);

                DrawLine(p1, p2);                
            }

            // Draw final line to reach edge.End;
            DrawLine(p2, (Vector2F)edge.End);
        }
    }
}
