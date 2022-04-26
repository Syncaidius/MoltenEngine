using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public partial class Shape
    {
        public class LinearEdge : Edge
        {
            public LinearEdge(Vector2D p0, Vector2D p1, EdgeColor color = EdgeColor.White) : base(color)
            {
                p = new Vector2D[] { p0, p1 };
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="param">A percentage value between 0.0 and 1.0.</param>
            /// <returns></returns>
            public override Vector2D Point(double param)
            {
                return Vector2D.Lerp(ref p[P0], ref p[P1], param);
            }

            public override Vector2D PointAlongEdge(double percentage)
            {
                return Vector2D.Lerp(ref p[P0], ref p[P1], percentage);
            }

            public override Vector2D GetDirection(double param)
            {
                return p[P1] - p[P0];
            }

            public override void SplitInThirds(ref Edge part1, ref Edge part2, ref Edge part3)
            {
                part1 = new LinearEdge(p[P0], Point(1 / 3.0), Color);
                part2 = new LinearEdge(Point(1 / 3.0), Point(2 / 3.0), Color);
                part3 = new LinearEdge(Point(2 / 3.0), p[P1], Color);
            }

            public unsafe override int ScanlineIntersections(double* x, int* dy, double y)
            {
                if ((y >= p[0].Y && y < p[1].Y) || (y >= p[1].Y && y < p[0].Y))
                {
                    double param = (y - p[0].Y) / (p[1].Y - p[0].Y);
                    x[0] = MathHelperDP.Lerp(p[0].X, p[1].X, param);
                    dy[0] = Math.Sign(p[1].Y - p[0].Y);
                    return 1;
                }
                return 0;
            }

            public override SignedDistance SignedDistance(Vector2D origin, out double param)
            {
                Vector2D aq = origin - p[0];
                Vector2D ab = p[1] - p[0];
                param = Vector2D.Dot(aq, ab) / Vector2D.Dot(ab, ab);
                int greaterThanHalf = param > 0.5 ? 1 : 0;

                Vector2D eq = p[greaterThanHalf] - origin;
                double endpointDistance = eq.Length();
                if (param > 0 && param < 1)
                {
                    double orthoDistance = Vector2D.Dot(ab.GetOrthonormal(false), aq);
                    if (Math.Abs(orthoDistance) < endpointDistance)
                        return new SignedDistance(orthoDistance, 0);
                }

                Vector2D abNormalized = Vector2D.Normalize(ab);
                Vector2D eqNormalized = Vector2D.Normalize(eq);
                return new SignedDistance(MathHelperDP.NonZeroSign(Vector2D.Cross(aq, ab)) * endpointDistance, Math.Abs(Vector2D.Dot(abNormalized, eqNormalized)));
            }
        }
    }
}
