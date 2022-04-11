using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public class CubicSegment : EdgeSegment
    {
        Vector2D[] p;

        public CubicSegment(Vector2D p0, Vector2D p1, Vector2D p2, Vector2D p3, EdgeColor color) : base(color)
        {
            p = new Vector2D[4];

            if ((p1 == p0 || p1 == p3) && (p2 == p0 || p2 == p3))
            {
                p1 = MsdfMath.Mix(p0, p3, 1 / 3.0);
                p2 = MsdfMath.Mix(p0, p3, 2 / 3.0);
            }
            p[0] = p0;
            p[1] = p1;
            p[2] = p2;
            p[3] = p3;
        }

        public void Deconverge(int param, double amount)
        {
            Vector2D dir = Direction(param);
            Vector2D normal = dir.GetOrthonormal();
            double h = Vector2D.Dot(DirectionChange(param) - dir, normal);
            switch (param)
            {
                case 0:
                    p[1] += amount * (dir + MsdfMath.Sign(h) * Math.Sqrt(Math.Abs(h)) * normal);
                    break;
                case 1:
                    p[2] -= amount * (dir - MsdfMath.Sign(h) * Math.Sqrt(Math.Abs(h)) * normal);
                    break;
            }
        }

        public unsafe override void Bound(ref double l, ref double b, ref double r, ref double t)
        {
            PointBounds(p[0], ref l, ref b, ref r, ref t);
            PointBounds(p[3], ref l, ref b, ref r, ref t);
            Vector2D a0 = p[1] - p[0];
            Vector2D a1 = 2 * (p[2] - p[1] - a0);
            Vector2D a2 = p[3] - 3 * p[2] + 3 * p[1] - p[0];
            double* parameters = stackalloc double[2];
            int solutions;
            solutions = EquationSolver.SolveQuadratic(parameters, a2.X, a1.X, a0.X);
            for (int i = 0; i < solutions; ++i)
                if (parameters[i] > 0 && parameters[i] < 1)
                    PointBounds(Point(parameters[i]), ref l, ref b, ref r, ref t);
            solutions = EquationSolver.SolveQuadratic(parameters, a2.Y, a1.Y, a0.Y);
            for (int i = 0; i < solutions; ++i)
                if (parameters[i] > 0 && parameters[i] < 1)
                    PointBounds(Point(parameters[i]), ref l, ref b, ref r, ref t);
        }

        public override EdgeSegment Clone()
        {
            return new CubicSegment(p[0], p[1], p[2], p[3], Color);
        }

        public override Vector2D Direction(double param)
        {
            Vector2D tangent = MsdfMath.Mix(MsdfMath.Mix(p[1] - p[0], p[2] - p[1], param), MsdfMath.Mix(p[2] - p[1], p[3] - p[2], param), param);
            if (tangent.X == 0 && tangent.Y == 0)
            {
                if (param == 0) return p[2] - p[0];
                if (param == 1) return p[3] - p[1];
            }
            return tangent;
        }

        public override Vector2D DirectionChange(double param)
        {
            return MsdfMath.Mix((p[2] - p[1]) - (p[1] - p[0]), (p[3] - p[2]) - (p[2] - p[1]), param);
        }

        public override void MoveEndPoint(Vector2D to)
        {
            p[2] += to - p[3];
            p[3] = to;
        }

        public override void MoveStartPoint(Vector2D to)
        {
            p[1] += to - p[0];
            p[0] = to;
        }

        public override Vector2D Point(double param)
        {
            Vector2D p12 = MsdfMath.Mix(p[1], p[2], param);
            return MsdfMath.Mix(MsdfMath.Mix(MsdfMath.Mix(p[0], p[1], param), p12, param), MsdfMath.Mix(p12, MsdfMath.Mix(p[2], p[3], param), param), param);
        }

        public override void Reverse()
        {
            Vector2D tmp = p[0];
            p[0] = p[3];
            p[3] = tmp;
            tmp = p[1];
            p[1] = p[2];
            p[2] = tmp;
        }

        public unsafe override int ScanlineIntersections(double* x, int* dy, double y)
        {
            int total = 0;
            int nextDY = y > p[0].Y ? 1 : -1;
            x[total] = p[0].X;
            if (p[0].Y == y)
            {
                if (p[0].Y < p[1].Y || (p[0].Y == p[1].Y && (p[0].Y < p[2].Y || (p[0].Y == p[2].Y && p[0].Y < p[3].Y))))
                    dy[total++] = 1;
                else
                    nextDY = 1;
            }
            {
                Vector2D ab = p[1] - p[0];
                Vector2D br = p[2] - p[1] - ab;
                Vector2D ass = (p[3] - p[2]) - (p[2] - p[1]) - br;
                double* t = stackalloc double[3];
                int solutions = EquationSolver.SolveCubic(t, ass.Y, 3 * br.Y, 3 * ab.Y, p[0].Y - y);
                // Sort solutions
                double tmp;
                if (solutions >= 2)
                {
                    if (t[0] > t[1])
                    {
                        tmp = t[0];
                        t[0] = t[1];
                        t[1] = tmp;
                    }

                    if (solutions >= 3 && t[1] > t[2])
                    {
                        tmp = t[1];
                        t[1] = t[2];
                        t[2] = tmp;
                        if (t[0] > t[1])
                        {
                            tmp = t[0];
                            t[0] = t[1];
                            t[1] = tmp;
                        }
                    }
                }
                for (int i = 0; i < solutions && total < 3; ++i)
                {
                    if (t[i] >= 0 && t[i] <= 1)
                    {
                        x[total] = p[0].X + 3 * t[i] * ab.X + 3 * t[i] * t[i] * br.X + t[i] * t[i] * t[i] * ass.X;
                        if (nextDY * (ab.Y + 2 * t[i] * br.Y + t[i] * t[i] * ass.Y) >= 0)
                        {
                            dy[total++] = nextDY;
                            nextDY = -nextDY;
                        }
                    }
                }
            }

            if (p[3].Y == y)
            {
                if (nextDY > 0 && total > 0)
                {
                    --total;
                    nextDY = -1;
                }
                if ((p[3].Y < p[2].Y || (p[3].Y == p[2].Y && (p[3].Y < p[1].Y || (p[3].Y == p[1].Y && p[3].Y < p[0].Y)))) && total < 3)
                {
                    x[total] = p[3].X;
                    if (nextDY < 0)
                    {
                        dy[total++] = -1;
                        nextDY = 1;
                    }
                }
            }
            if (nextDY != (y >= p[3].Y ? 1 : -1))
            {
                if (total > 0)
                    --total;
                else
                {
                    if (Math.Abs(p[3].Y - y) < Math.Abs(p[0].Y - y))
                        x[total] = p[3].X;
                    dy[total++] = nextDY;
                }
            }
            return total;
        }

        public override SignedDistance SignedDistance(Vector2D origin, out double param)
        {
            const int MSDFGEN_CUBIC_SEARCH_STARTS = 4;
            const int MSDFGEN_CUBIC_SEARCH_STEPS = 4;

            Vector2D qa = p[0] - origin;
            Vector2D ab = p[1] - p[0];
            Vector2D br = p[2] - p[1] - ab;
            Vector2D ass = (p[3] - p[2]) - (p[2] - p[1]) - br;

            Vector2D epDir = Direction(0);
            double minDistance = MsdfMath.NonZeroSign(Vector2D.Cross(epDir, qa)) * qa.Length(); // distance from A
            param = -Vector2D.Dot(qa, epDir) / Vector2D.Dot(epDir, epDir);
            {
                epDir = Direction(1);
                double distance = (p[3] - origin).Length(); // distance from B
                if (distance < Math.Abs(minDistance))
                {
                    minDistance = MsdfMath.NonZeroSign(Vector2D.Cross(epDir, p[3] - origin)) * distance;
                    param = Vector2D.Dot(epDir - (p[3] - origin), epDir) / Vector2D.Dot(epDir, epDir);
                }
            }
            // Iterative minimum distance search
            for (int i = 0; i <= MSDFGEN_CUBIC_SEARCH_STARTS; ++i)
            {
                double t = (double)i / MSDFGEN_CUBIC_SEARCH_STARTS;
                Vector2D qe = qa + 3 * t * ab + 3 * t * t * br + t * t * t * ass;
                for (int step = 0; step < MSDFGEN_CUBIC_SEARCH_STEPS; ++step)
                {
                    // Improve t
                    Vector2D d1 = 3 * ab + 6 * t * br + 3 * t * t * ass;
                    Vector2D d2 = 6 * br + 6 * t * ass;
                    t -= Vector2D.Dot(qe, d1) / (Vector2D.Dot(d1, d1) + Vector2D.Dot(qe, d2));
                    if (t <= 0 || t >= 1)
                        break;
                    qe = qa + 3 * t * ab + 3 * t * t * br + t * t * t * ass;
                    double distance = qe.Length();
                    if (distance < Math.Abs(minDistance))
                    {
                        minDistance = MsdfMath.NonZeroSign(Vector2D.Cross(d1, qe)) * distance;
                        param = t;
                    }
                }
            }

            if (param >= 0 && param <= 1)
                return new SignedDistance(minDistance, 0);
            if (param < .5)
                return new SignedDistance(minDistance, Math.Abs(Vector2D.Dot(Direction(0).GetNormalized(), qa.GetNormalized())));
            else
                return new SignedDistance(minDistance, Math.Abs(Vector2D.Dot(Direction(1).GetNormalized(), (p[3] - origin).GetNormalized())));
        }

        public override void SplitInThirds(ref EdgeSegment part1, ref EdgeSegment part2, ref EdgeSegment part3)
        {
            part1 = new CubicSegment(p[0], p[0] == p[1] ? p[0] : MsdfMath.Mix(p[0], p[1], 1 / 3.0), MsdfMath.Mix(MsdfMath.Mix(p[0], p[1], 1 / 3.0), MsdfMath.Mix(p[1], p[2], 1 / 3.0), 1 / 3.0), Point(1 / 3.0), Color);
            part2 = new CubicSegment(Point(1 / 3.0),
                MsdfMath.Mix(MsdfMath.Mix(MsdfMath.Mix(p[0], p[1], 1 / 3.0), MsdfMath.Mix(p[1], p[2], 1 / 3.0), 1 / 3.0), MsdfMath.Mix(MsdfMath.Mix(p[1], p[2], 1 / 3.0), MsdfMath.Mix(p[2], p[3], 1 / 3.0), 1 / 3.0), 2 / 3.0),
                MsdfMath.Mix(MsdfMath.Mix(MsdfMath.Mix(p[0], p[1], 2 / 3.0), MsdfMath.Mix(p[1], p[2], 2 / 3.0), 2 / 3.0), MsdfMath.Mix(MsdfMath.Mix(p[1], p[2], 2 / 3.0), MsdfMath.Mix(p[2], p[3], 2 / 3.0), 2 / 3.0), 1 / 3.0),
                Point(2 / 3.0), Color);
            part3 = new CubicSegment(Point(2 / 3.0), MsdfMath.Mix(MsdfMath.Mix(p[1], p[2], 2 / 3.0), MsdfMath.Mix(p[2], p[3], 2 / 3.0), 2 / 3.0), p[2] == p[3] ? p[3] : MsdfMath.Mix(p[2], p[3], 2 / 3.0), p[3], Color);
        }
    }
}
