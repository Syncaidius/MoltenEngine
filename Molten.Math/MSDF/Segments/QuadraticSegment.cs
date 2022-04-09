using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.MSDF
{
    public class QuadraticSegment : EdgeSegment
    {
        Vector2D[] p;
        public QuadraticSegment(Vector2D p0, Vector2D p1, Vector2D p2, EdgeColor color = EdgeColor.WHITE) : base(color)
        {
            if (p1 == p0 || p1 == p2)
                p1 = 0.5 * (p0 + p2);

            p = new Vector2D[3] { p0, p1, p2 };
        }

        public EdgeSegment ConvertToCubic()
        {
            return new CubicSegment(p[0], MsdfMath.mix(p[0], p[1], 2 / 3.0), MsdfMath.mix(p[1], p[2], 1 / 3.0), p[2], Color);
        }

        public override void bound(ref double l, ref double b, ref double r, ref double t)
        {
            pointBounds(p[0], ref l, ref b, ref r, ref t);
            pointBounds(p[2], ref l, ref b, ref r, ref t);
            Vector2D bot = (p[1] - p[0]) - (p[2] - p[1]);
            if (bot.X != 0)
            {
                double param = (p[1].X - p[0].X) / bot.X;
                if (param > 0 && param < 1)
                    pointBounds(point(param), ref l, ref b, ref r, ref t);
            }
            if (bot.Y != 0)
            {
                double param = (p[1].Y - p[0].Y) / bot.Y;
                if (param > 0 && param < 1)
                    pointBounds(point(param), ref l, ref b, ref r, ref t);
            }
        }

        public override EdgeSegment Clone()
        {
            return new QuadraticSegment(p[0], p[1], p[2], Color);
        }

        public override Vector2D direction(double param)
        {
            Vector2D tangent = MsdfMath.mix(p[1] - p[0], p[2] - p[1], param);
            if (tangent.X == 0 && tangent.Y == 0)
                return p[2] - p[0];
            return tangent;
        }

        public override Vector2D directionChange(double param)
        {
            return (p[2] - p[1]) - (p[1] - p[0]);
        }

        public override void moveEndPoint(Vector2D to)
        {
            Vector2D origEDir = p[2] - p[1];
            Vector2D origP1 = p[1];
            p[1] += Vector2D.Cross(p[2] - p[1], to - p[2]) / Vector2D.Cross(p[2] - p[1], p[0] - p[1]) * (p[0] - p[1]);
            p[2] = to;
            if (Vector2D.Dot(origEDir, p[2] - p[1]) < 0)
                p[1] = origP1;
        }

        public override void moveStartPoint(Vector2D to)
        {
            Vector2D origSDir = p[0] - p[1];
            Vector2D origP1 = p[1];
            p[1] += Vector2D.Cross(p[0] - p[1], to - p[0]) / Vector2D.Cross(p[0] - p[1], p[2] - p[1]) * (p[2] - p[1]);
            p[0] = to;
            if (Vector2D.Dot(origSDir, p[0] - p[1]) < 0)
                p[1] = origP1;
        }

        public override Vector2D point(double param)
        {
            return MsdfMath.mix(MsdfMath.mix(p[0], p[1], param), MsdfMath.mix(p[1], p[2], param), param);
        }

        public override void reverse()
        {
            Vector2D tmp = p[0];
            p[0] = p[2];
            p[2] = tmp;
        }

        public unsafe override int scanlineIntersections(double* x, int* dy, double y)
        {
            int total = 0;
            int nextDY = y > p[0].Y ? 1 : -1;
            x[total] = p[0].X;
            if (p[0].Y == y)
            {
                if (p[0].Y < p[1].Y || (p[0].Y == p[1].Y && p[0].Y < p[2].Y))
                    dy[total++] = 1;
                else
                    nextDY = 1;
            }
            {
                Vector2D ab = p[1] - p[0];
                Vector2D br = p[2] - p[1] - ab;
                double* t = stackalloc double[2];
                int solutions = EquationSolver.solveQuadratic(t, br.Y, 2 * ab.Y, p[0].Y - y);
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
                        x[total] = p[0].X + 2 * t[i] * ab.X + t[i] * t[i] * br.X;
                        if (nextDY * (ab.Y + t[i] * br.Y) >= 0)
                        {
                            dy[total++] = nextDY;
                            nextDY = -nextDY;
                        }
                    }
                }
            }
            if (p[2].Y == y)
            {
                if (nextDY > 0 && total > 0)
                {
                    --total;
                    nextDY = -1;
                }
                if ((p[2].Y < p[1].Y || (p[2].Y == p[1].Y && p[2].Y < p[0].Y)) && total < 2)
                {
                    x[total] = p[2].X;
                    if (nextDY < 0)
                    {
                        dy[total++] = -1;
                        nextDY = 1;
                    }
                }
            }
            if (nextDY != (y >= p[2].Y ? 1 : -1))
            {
                if (total > 0)
                    --total;
                else
                {
                    if (Math.Abs(p[2].Y - y) < Math.Abs(p[0].Y - y))
                        x[total] = p[2].X;
                    dy[total++] = nextDY;
                }
            }
            return total;
        }

        public unsafe override SignedDistance signedDistance(Vector2D origin, out double param)
        {
            Vector2D qa = p[0] - origin;
            Vector2D ab = p[1] - p[0];
            Vector2D br = p[2] - p[1] - ab;
            double a = Vector2D.Dot(br, br);
            double b = 3 * Vector2D.Dot(ab, br);
            double c = 2 * Vector2D.Dot(ab, ab) + Vector2D.Dot(qa, br);
            double d = Vector2D.Dot(qa, ab);
            double* t = stackalloc double[3];
            int solutions = EquationSolver.solveCubic(t, a, b, c, d);

            Vector2D epDir = direction(0);
            double minDistance = MsdfMath.nonZeroSign(Vector2D.Cross(epDir, qa)) * qa.Length(); // distance from A
            param = -Vector2D.Dot(qa, epDir) / Vector2D.Dot(epDir, epDir);
            {
                epDir = direction(1);
                double distance = (p[2] - origin).Length(); // distance from B
                if (distance < Math.Abs(minDistance))
                {
                    minDistance = MsdfMath.nonZeroSign(Vector2D.Cross(epDir, p[2] - origin)) * distance;
                    param = Vector2D.Dot(origin - p[1], epDir) / Vector2D.Dot(epDir, epDir);
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
                        minDistance = MsdfMath.nonZeroSign(Vector2D.Cross(ab + t[i] * br, qe)) * distance;
                        param = t[i];
                    }
                }
            }

            if (param >= 0 && param <= 1)
                return new SignedDistance(minDistance, 0);
            if (param < .5)
            {
                Vector2D d0n = direction(0).GetNormalized();
                Vector2D qan = qa.GetNormalized();
                return new SignedDistance(minDistance, Math.Abs(Vector2D.Dot(d0n, qan)));
            }
            else
            {
                Vector2D d1n = direction(1).GetNormalized();
                Vector2D p2n = (p[2] - origin).GetNormalized();
                return new SignedDistance(minDistance, Math.Abs(Vector2D.Dot(d1n, p2n)));
            }
        }

        public override void splitInThirds(ref EdgeSegment part1, ref EdgeSegment part2, ref EdgeSegment part3)
        {
            part1 = new QuadraticSegment(p[0], MsdfMath.mix(p[0], p[1], 1 / 3.0), point(1 / 3.0), Color);
            part2 = new QuadraticSegment(point(1 / 3.0), MsdfMath.mix(MsdfMath.mix(p[0], p[1], 5 / 9.0), MsdfMath.mix(p[1], p[2], 4 / 9.0), .5), point(2 / 3.0), Color);
            part3 = new QuadraticSegment(point(2 / 3.0), MsdfMath.mix(p[1], p[2], 2 / 3.0), p[2], Color);
        }

        public double length()
        {
            Vector2D ab = p[1] - p[0];
            Vector2D br = p[2] - p[1] - ab;
            double abab = Vector2D.Dot(ab, ab);
            double abbr = Vector2D.Dot(ab, br);
            double brbr = Vector2D.Dot(br, br);
            double abLen = Math.Sqrt(abab);
            double brLen = Math.Sqrt(brbr);
            double crs = Vector2D.Cross(ab, br);
            double h = Math.Sqrt(abab + abbr + abbr + brbr);
            return (
                brLen * ((abbr + brbr) * h - abbr * abLen) +
                crs * crs * Math.Log((brLen * h + abbr + brbr) / (brLen * abLen + abbr))
            ) / (brbr * brLen);
        }
    }
}
