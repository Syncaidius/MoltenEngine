using Molten.DoublePrecision;

namespace Molten.Graphics
{
    public partial class SpriteBatcher
    {
        /// <summary>Draws .</summary>
        /// <param name="edge">The edge to draw.</param>
        public void DrawEdge(Shape.Edge edge, Color color, float thickness, float resolution = 16, uint surfaceSlice = 0)
        {
            LineStyle style = new LineStyle(color, color, thickness);
            DrawEdge(edge, ref style, resolution, surfaceSlice);
        }

        /// <summary>Draws .</summary>
        /// <param name="edge">The edge to draw.</param>
        public void DrawEdge(Shape.Edge edge, ref LineStyle style, float resolution = 16, uint surfaceSlice = 0)
        {
            // Early exit for linear edges. Always straight.
            if (edge is Shape.LinearEdge)
            {
                DrawLine((Vector2F)edge.P[0], (Vector2F)edge.P[1], ref style, surfaceSlice);
                return;
            }

            double increment = 1.0 / resolution;
            Vector2F p1;
            Vector2F p2 = (Vector2F)edge.Point(0);

            for (int i = 0; i < resolution; i++)
            {
                p1 = p2;
                p2 = (Vector2F)edge.Point(increment * i);

                DrawLine(p1, p2, ref style, surfaceSlice);                
            }

            // Draw final line to reach edge.End;
            DrawLine(p2, (Vector2F)edge.End, ref style, surfaceSlice);
        }

        /// <summary>
        /// Draws a <see cref="BezierCurve2D"/>.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="isCubic">If true, the curve will be drawn as a cubic curve. If false, a quadratic curve will be drawn.</param>
        /// <param name="resolution">The number of points that make up the curve.</param>
        public void DrawEdge(BezierCurve2D curve, ref LineStyle style, bool isCubic, float resolution = 16, uint surfaceSlice = 0)
        {
            float increment = 1f / resolution;
            Vector2F p1;
            Vector2F p2 = curve.Start;

            for (int i = 0; i < resolution; i++)
            {
                p1 = p2;
                p2 = isCubic ?
                    BezierCurve2D.GetCubicPoint((increment * i), curve) :
                    BezierCurve2D.GetQuadraticPoint((increment * i), curve);

                DrawLine(p1, p2, ref style, surfaceSlice);
            }

            // Draw final line to reach edge.End;
            DrawLine(p2, curve.End, ref style, surfaceSlice);
        }
    }
}
