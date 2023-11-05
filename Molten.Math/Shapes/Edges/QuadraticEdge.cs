using Molten.DoublePrecision;

namespace Molten.Shapes
{
    public class QuadraticEdge : Edge
    {
        public QuadraticEdge(Vector2D p0, Vector2D p1, Vector2D pControl, EdgeColor color = EdgeColor.White) : base(color)
        {
            if (p1 == p0 || p1 == pControl)
                p1 = 0.5 * (p0 + pControl);

            P = new Vector2D[] { p0, pControl, p1 };
        }

        public override ref Vector2D Start => ref P[0];

        public override ref Vector2D End => ref P[2];

        public ref Vector2D ControlPoint => ref P[1];

        /// <summary>
        /// Gets a point along the quadratic edge
        /// </summary>
        /// <param name="percent"></param>
        /// <returns></returns>
        public override Vector2D Point(double percent)
        {
            Vector2D start = Vector2D.Lerp(ref P[0], ref P[1], percent);
            Vector2D end = Vector2D.Lerp(ref P[1], ref P[2], percent);
            return Vector2D.Lerp(ref start, ref end, percent);
        }

        public override Vector2D PointAlongEdge(double percentage)
        {
            return BezierCurve2D.GetQuadraticPoint(percentage, Start, End, ControlPoint);
        }

        public CubicEdge ConvertToCubic()
        {
            return new CubicEdge(P[0], Vector2D.Lerp(P[0], P[1], 2 / 3.0), Vector2D.Lerp(P[1], P[2], 1 / 3.0), P[2], Color);
        }

        public override Vector2D GetDirection(double param)
        {
            Vector2D tangent = Vector2D.Lerp(P[1] - P[0], P[2] - P[1], param);
            if (tangent.X == 0 && tangent.Y == 0)
                return P[2] - P[0];
            return tangent;
        }

        public override void SplitInThirds(ref Edge part1, ref Edge part2, ref Edge part3)
        {
            part1 = new QuadraticEdge(P[0], Vector2D.Lerp(ref P[0], ref P[1], 1 / 3.0), Point(1 / 3.0), Color);
            part2 = new QuadraticEdge(Point(1 / 3.0), Vector2D.Lerp(Vector2D.Lerp(ref P[0], ref P[1], 5 / 9.0), Vector2D.Lerp(ref P[1], ref P[2], 4 / 9.0), .5), Point(2 / 3.0), Color);
            part3 = new QuadraticEdge(Point(2 / 3.0), Vector2D.Lerp(ref P[1], ref P[2], 2 / 3.0), P[2], Color);
        }

        public unsafe override int ScanlineIntersections(double* x, int* dy, double y)
        {
            int total = 0;
            int nextDY = y > P[0].Y ? 1 : -1;
            x[total] = P[0].X;
            if (P[0].Y == y)
            {
                if (P[0].Y < P[1].Y || (P[0].Y == P[1].Y && P[0].Y < P[2].Y))
                    dy[total++] = 1;
                else
                    nextDY = 1;
            }

            Vector2D ab = P[1] - P[0];
            Vector2D br = P[2] - P[1] - ab;
            double* t = stackalloc double[2];
            int solutions = SignedDistanceSolver.SolveQuadratic(t, br.Y, 2 * ab.Y, P[0].Y - y);
            // Sort solutions
            double tmp;
            if (solutions >= 2 && t[0] > t[1])
            {
                tmp = t[0];
                t[0] = t[1];
                t[1] = tmp;
            }

            for (int i = 0; i < solutions && total < 2; ++i)
            {
                if (t[i] >= 0 && t[i] <= 1)
                {
                    x[total] = P[0].X + 2 * t[i] * ab.X + t[i] * t[i] * br.X;
                    if (nextDY * (ab.Y + t[i] * br.Y) >= 0)
                    {
                        dy[total++] = nextDY;
                        nextDY = -nextDY;
                    }
                }
            }

            if (P[2].Y == y)
            {
                if (nextDY > 0 && total > 0)
                {
                    --total;
                    nextDY = -1;
                }
                if ((P[2].Y < P[1].Y || (P[2].Y == P[1].Y && P[2].Y < P[0].Y)) && total < 2)
                {
                    x[total] = P[2].X;
                    if (nextDY < 0)
                    {
                        dy[total++] = -1;
                        nextDY = 1;
                    }
                }
            }

            if (nextDY != (y >= P[2].Y ? 1 : -1))
            {
                if (total > 0)
                    --total;
                else
                {
                    if (Math.Abs(P[2].Y - y) < Math.Abs(P[0].Y - y))
                        x[total] = P[2].X;
                    dy[total++] = nextDY;
                }
            }

            return total;
        }

        public unsafe override SignedDistance SignedDistance(Vector2D origin, out double param)
        {
            Vector2D qa = P[0] - origin;
            Vector2D ab = P[1] - P[0];
            Vector2D br = P[2] - P[1] - ab;
            double a = Vector2D.Dot(br, br);
            double b = 3 * Vector2D.Dot(ab, br);
            double c = 2 * Vector2D.Dot(ab, ab) + Vector2D.Dot(qa, br);
            double d = Vector2D.Dot(qa, ab);
            double* t = stackalloc double[3];
            int solutions = SignedDistanceSolver.SolveCubic(t, a, b, c, d);

            Vector2D epDir = GetDirection(0);
            double minDistance = MathHelper.NonZeroSign(Vector2D.Cross(epDir, qa)) * qa.Length(); // distance from A
            param = -Vector2D.Dot(qa, epDir) / Vector2D.Dot(epDir, epDir);
            {
                epDir = GetDirection(1);
                double distance = (P[2] - origin).Length(); // distance from B
                if (distance < Math.Abs(minDistance))
                {
                    minDistance = MathHelper.NonZeroSign(Vector2D.Cross(epDir, P[2] - origin)) * distance;
                    param = Vector2D.Dot(origin - P[1], epDir) / Vector2D.Dot(epDir, epDir);
                }
            }
            for (int i = 0; i < solutions; ++i)
            {
                if (t[i] > 0 && t[i] < 1)
                {
                    Vector2D qe = qa + 2 * t[i] * ab + t[i] * t[i] * br;
                    double distance = qe.Length();
                    if (distance <= Math.Abs(minDistance))
                    {
                        minDistance = MathHelper.NonZeroSign(Vector2D.Cross(ab + t[i] * br, qe)) * distance;
                        param = t[i];
                    }
                }
            }

            if (param >= 0 && param <= 1)
                return new SignedDistance(minDistance, 0);
            if (param < .5)
            {
                Vector2D d0n = GetDirection(0).GetNormalized();
                Vector2D qan = qa.GetNormalized();
                return new SignedDistance(minDistance, Math.Abs(Vector2D.Dot(d0n, qan)));
            }
            else
            {
                Vector2D d1n = GetDirection(1).GetNormalized();
                Vector2D p2n = (P[2] - origin).GetNormalized();
                return new SignedDistance(minDistance, Math.Abs(Vector2D.Dot(d1n, p2n)));
            }
        }
    }
}
