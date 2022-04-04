using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
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

        public override void bound(ref double l, ref double b, ref double r, ref double t)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public override void moveStartPoint(Vector2D to)
        {
            throw new NotImplementedException();
        }

        public override Vector2D point(double param)
        {
            return MsdfMath.mix(MsdfMath.mix(p[0], p[1], param), MsdfMath.mix(p[1], p[2], param), param);
        }

        public override void reverse()
        {
            throw new NotImplementedException();
        }

        public override int scanlineIntersections(X3 x, DY3 dy, double y)
        {
            throw new NotImplementedException();
        }

        public override SignedDistance signedDistance(Vector2D origin, out double param)
        {
            Vector2D qa = p[0] - origin;
            Vector2D ab = p[1] - p[0];
            Vector2D br = p[2] - p[1] - ab;
            double a = Vector2D.Dot(br, br);
            double b = 3 * Vector2D.Dot(ab, br);
            double c = 2 * Vector2D.Dot(ab, ab) + Vector2D.Dot(qa, br);
            double d = Vector2D.Dot(qa, ab);
            X3 t;
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
                return new SignedDistance(minDistance, Math.Abs(Vector2D.Dot(direction(0).normalize(), qa.normalize())));
            else
                return new SignedDistance(minDistance, Math.Abs(Vector2D.Dot(direction(1).normalize(), (p[2] - origin).normalize())));
        }

        public override void splitInThirds(ref EdgeSegment part1, ref EdgeSegment part2, ref EdgeSegment part3)
        {
            throw new NotImplementedException();
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
