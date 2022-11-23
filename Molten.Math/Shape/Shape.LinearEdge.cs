using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.DoublePrecision;

namespace Molten
{
    public partial class Shape
    {
        public class LinearEdge : Edge
        {
            public LinearEdge(Vector2D p0, Vector2D p1, EdgeColor color = EdgeColor.White) : base(color)
            {
                P = new Vector2D[] { p0, p1 };
            }

            public override ref Vector2D Start => ref P[0];

            public override ref Vector2D End => ref P[1];

            /// <summary>
            /// 
            /// </summary>
            /// <param name="param">A percentage value between 0.0 and 1.0.</param>
            /// <returns></returns>
            public override Vector2D Point(double param)
            {
                return Vector2D.Lerp(ref P[0], ref P[1], param);
            }

            public override Vector2D PointAlongEdge(double percentage)
            {
                return Vector2D.Lerp(ref P[0], ref P[1], percentage);
            }

            public override Vector2D GetDirection(double param)
            {
                return P[1] - P[0];
            }

            public override void SplitInThirds(ref Edge part1, ref Edge part2, ref Edge part3)
            {
                part1 = new LinearEdge(P[0], Point(1 / 3.0), Color);
                part2 = new LinearEdge(Point(1 / 3.0), Point(2 / 3.0), Color);
                part3 = new LinearEdge(Point(2 / 3.0), P[1], Color);
            }

            public unsafe override int ScanlineIntersections(double* x, int* dy, double y)
            {
                if ((y >= P[0].Y && y < P[1].Y) || (y >= P[1].Y && y < P[0].Y))
                {
                    double param = (y - P[0].Y) / (P[1].Y - P[0].Y);
                    x[0] = MathHelper.Lerp(P[0].X, P[1].X, param);
                    dy[0] = Math.Sign(P[1].Y - P[0].Y);
                    return 1;
                }
                return 0;
            }

            public override SignedDistance SignedDistance(Vector2D origin, out double param)
            {
                Vector2D aq = origin - P[0];
                Vector2D ab = P[1] - P[0];
                param = Vector2D.Dot(aq, ab) / Vector2D.Dot(ab, ab);
                int greaterThanHalf = param > 0.5 ? 1 : 0;

                Vector2D eq = P[greaterThanHalf] - origin;
                double endpointDistance = eq.Length();
                if (param > 0 && param < 1)
                {
                    double orthoDistance = Vector2D.Dot(ab.GetOrthonormal(false), aq);
                    if (Math.Abs(orthoDistance) < endpointDistance)
                        return new SignedDistance(orthoDistance, 0);
                }

                Vector2D abNormalized = Vector2D.Normalize(ab);
                Vector2D eqNormalized = Vector2D.Normalize(eq);
                return new SignedDistance(MathHelper.NonZeroSign(Vector2D.Cross(aq, ab)) * endpointDistance, Math.Abs(Vector2D.Dot(abNormalized, eqNormalized)));
            }
        }
    }
}
