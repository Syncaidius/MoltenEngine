﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public partial class ContourShape
    {
        public class QuadraticEdge : Edge
        {
            public QuadraticEdge(Vector2D p0, Vector2D p1, Vector2D pControl, EdgeColor color = EdgeColor.White) : base(color)
            {
                p = new Vector2D[] { p0, p1, pControl };
            }

            public override Vector2D Point(double param)
            {
                Vector2D start = Vector2D.Lerp(ref p[P0], ref p[P1], param);
                Vector2D end = Vector2D.Lerp(ref p[P1], ref p[CP1], param);
                return Vector2D.Lerp(ref start, ref end, param);
            }

            public override Vector2D PointAlongEdge(double percentage)
            {
                return BezierCurve2D.CalculateQuadratic(percentage, p[P0], p[P1], p[CP1]);
            }

            public CubicEdge ConvertToCubic()
            {
                return new CubicEdge(p[P0], Vector2D.Lerp(p[P0], p[P1], 2 / 3.0), Vector2D.Lerp(p[P1], p[CP1], 1 / 3.0), p[2], Color);
            }

            public override Vector2D GetDirection(double param)
            {
                Vector2D tangent = Vector2D.Lerp(p[P1] - p[P0], p[CP1] - p[P1], param);
                if (tangent.X == 0 && tangent.Y == 0)
                    return p[CP1] - p[P0];
                return tangent;
            }

            public override Vector2D GetDirectionChange(double param)
            {
                return (p[2] - p[1]) - (p[1] - p[0]);
            }

            public override void SplitInThirds(ref Edge part1, ref Edge part2, ref Edge part3)
            {
                part1 = new QuadraticEdge(p[P0], Vector2D.Lerp(ref p[P0], ref p[P1], 1 / 3.0), Point(1 / 3.0), Color);
                part2 = new QuadraticEdge(Point(1 / 3.0), Vector2D.Lerp(Vector2D.Lerp(ref p[P0], ref p[P1], 5 / 9.0), Vector2D.Lerp(ref p[P1], ref p[CP1], 4 / 9.0), .5), Point(2 / 3.0), Color);
                part3 = new QuadraticEdge(Point(2 / 3.0), Vector2D.Lerp(ref p[P1], ref p[CP1], 2 / 3.0), p[CP1], Color);
            }

            public unsafe override int ScanlineIntersections(double* x, int* dy, double y)
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
                    int solutions = SignedDistanceSolver.SolveQuadratic(t, br.Y, 2 * ab.Y, p[0].Y - y);
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

            public unsafe override SignedDistance SignedDistance(Vector2D origin, out double param)
            {
                Vector2D qa = p[0] - origin;
                Vector2D ab = p[1] - p[0];
                Vector2D br = p[2] - p[1] - ab;
                double a = Vector2D.Dot(br, br);
                double b = 3 * Vector2D.Dot(ab, br);
                double c = 2 * Vector2D.Dot(ab, ab) + Vector2D.Dot(qa, br);
                double d = Vector2D.Dot(qa, ab);
                double* t = stackalloc double[3];
                int solutions = SignedDistanceSolver.SolveCubic(t, a, b, c, d);

                Vector2D epDir = GetDirection(0);
                double minDistance = MathHelperDP.NonZeroSign(Vector2D.Cross(epDir, qa)) * qa.Length(); // distance from A
                param = -Vector2D.Dot(qa, epDir) / Vector2D.Dot(epDir, epDir);
                {
                    epDir = GetDirection(1);
                    double distance = (p[2] - origin).Length(); // distance from B
                    if (distance < Math.Abs(minDistance))
                    {
                        minDistance = MathHelperDP.NonZeroSign(Vector2D.Cross(epDir, p[2] - origin)) * distance;
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
                            minDistance = MathHelperDP.NonZeroSign(Vector2D.Cross(ab + t[i] * br, qe)) * distance;
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
                    Vector2D p2n = (p[2] - origin).GetNormalized();
                    return new SignedDistance(minDistance, Math.Abs(Vector2D.Dot(d1n, p2n)));
                }
            }

            public double Length()
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
}