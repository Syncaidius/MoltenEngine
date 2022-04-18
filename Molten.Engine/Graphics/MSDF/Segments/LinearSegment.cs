using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public class LinearSegment : EdgeSegment
    {
        Vector2D[] p;

        public LinearSegment(Vector2D p0, Vector2D p1, EdgeColor edgeColor = EdgeColor.White) :
            base(edgeColor)
        {
            p = new Vector2D[] { p0, p1 };
        }

        public override void Bound(ref double l, ref double b, ref double r, ref double t)
        {
            PointBounds(p[0], ref l, ref b, ref r, ref t);
            PointBounds(p[1], ref l, ref b, ref r, ref t);
        }

        public override EdgeSegment Clone()
        {
            return new LinearSegment(p[0], p[1], Color);
        }

        public override Vector2D Direction(double param)
        {
            return p[1] - p[0];
        }

        public override Vector2D DirectionChange(double param)
        {
            return new Vector2D();
        }

        public override void MoveEndPoint(Vector2D to)
        {
            p[1] = to;
        }

        public override void MoveStartPoint(Vector2D to)
        {
            p[0] = to;
        }

        public override Vector2D Point(double param)
        {
            return Vector2D.Lerp(ref p[0], ref p[1], param);
        }

        public override void Reverse()
        {
            Vector2D tmp = p[0];
            p[0] = p[1];
            p[1] = tmp;
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

        public override void SplitInThirds(ref EdgeSegment part1, ref EdgeSegment part2, ref EdgeSegment part3)
        {
            part1 = new LinearSegment(p[0], Point(1 / 3.0), Color);
            part2 = new LinearSegment(Point(1 / 3.0), Point(2 / 3.0), Color);
            part3 = new LinearSegment(Point(2 / 3.0), p[1], Color);
        }

        public double Length()
        {
            return (p[1] - p[0]).Length();
        }
    }
}
