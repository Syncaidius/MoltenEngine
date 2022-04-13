using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public partial class ContourShape
    {
        public abstract class Edge
        {
            public const int P0 = 0;
            public const int P1 = 1;
            public const int CP1 = 2;
            public const int CP2 = 3;

            public Vector2D[] Points { get; protected init; }

            public abstract Vector2D Point(double param);

            public abstract Vector2D PointAlongEdge(double percentage);
        }

        public class LinearEdge : Edge
        {
            public LinearEdge(Vector2D p0, Vector2D p1)
            {
                Points = new Vector2D[] { p0, p1 };
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="param">A percentage value between 0.0 and 1.0.</param>
            /// <returns></returns>
            public override Vector2D Point(double param)
            {
                return Vector2D.Lerp(ref Points[P0], ref Points[P1], param);
            }

            public override Vector2D PointAlongEdge(double percentage)
            {
                return Vector2D.Lerp(ref Points[P0], ref Points[P1], percentage);
            }
        }

        public class QuadraticEdge : Edge
        {
            public QuadraticEdge(Vector2D p0, Vector2D p1, Vector2D pControl)
            {
                Points = new Vector2D[] { p0, p1, pControl };
            }

            public override Vector2D Point(double param)
            {
                Vector2D start = Vector2D.Lerp(ref Points[P0], ref Points[P1], param);
                Vector2D end = Vector2D.Lerp(ref Points[P1], ref Points[CP1], param);
                return Vector2D.Lerp(ref start, ref end, param);
            }

            public override Vector2D PointAlongEdge(double percentage)
            {
                return BezierCurve2D.CalculateQuadratic(percentage, Points[P0], Points[P1], Points[CP1]);
            }
        }

        public class CubicEdge : Edge
        {
            public CubicEdge(Vector2D p0, Vector2D p1, Vector2D pControl1, Vector2D pControl2)
            {
                Points = new Vector2D[] { p0, p1, pControl1, pControl2 };
            }

            public override Vector2D Point(double param)
            {
                Vector2D p12 = Vector2D.Lerp(ref Points[P1], ref Points[CP1], param);
                Vector2D start = Vector2D.Lerp(Vector2D.Lerp(ref Points[P0], ref Points[P1], param), p12, param);
                Vector2D end = Vector2D.Lerp(p12, Vector2D.Lerp(Points[CP1], Points[CP2], param), param);

                return Vector2D.Lerp(start, end, param);
            }

            public override Vector2D PointAlongEdge(double percentage)
            {
                return BezierCurve2D.CalculateCubic(percentage, Points[P0], Points[P1], Points[CP1], Points[CP2]);
            }
        }
    }
}
