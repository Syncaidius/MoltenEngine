using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public partial class Shape
    {
        public class CubicEdge : Edge
        {
            public CubicEdge(Vector2D p0, Vector2D p1, Vector2D pControl1, Vector2D pControl2, EdgeColor color = EdgeColor.White) : base(color)
            {
                p = new Vector2D[] { p0, p1, pControl1, pControl2 };
            }

            public override Vector2D Point(double param)
            {
                Vector2D p12 = Vector2D.Lerp(ref p[P1], ref p[CP1], param);
                Vector2D start = Vector2D.Lerp(Vector2D.Lerp(ref p[P0], ref p[P1], param), p12, param);
                Vector2D end = Vector2D.Lerp(p12, Vector2D.Lerp(p[CP1], p[CP2], param), param);

                return Vector2D.Lerp(start, end, param);
            }

            public override Vector2D PointAlongEdge(double percentage)
            {
                return BezierCurve2D.CalculateCubic(percentage, p[P0], p[P1], p[CP1], p[CP2]);
            }

            public override Vector2D GetDirection(double param)
            {
                Vector2D start = Vector2D.Lerp(p[P1] - p[P0], p[CP1] - p[P1], param);
                Vector2D end = Vector2D.Lerp(p[CP1] - p[P1], p[CP2] - p[CP1], param);
                Vector2D tangent = Vector2D.Lerp(ref start, ref end, param);

                if (tangent.X == 0 && tangent.Y == 0)
                {
                    if (param == 0)
                        return p[CP1] - p[P0];

                    if (param == 1)
                        return p[CP2] - p[P1];
                }

                return tangent;
            }

            public Vector2D GetDirectionChange(double param)
            {
                Vector2D start = (p[2] - p[1]) - (p[1] - p[0]);
                Vector2D end = (p[3] - p[2]) - (p[2] - p[1]);
                return Vector2D.Lerp(start, end, param);
            }

            public override void SplitInThirds(ref Edge part1, ref Edge part2, ref Edge part3)
            {
                part1 = new CubicEdge(p[P0], p[P0] == p[P1] ? p[P0] : Vector2D.Lerp(p[P0], p[P1], 1 / 3.0), Vector2D.Lerp(Vector2D.Lerp(p[P0], p[P1], 1 / 3.0), Vector2D.Lerp(p[P1], p[CP1], 1 / 3.0), 1 / 3.0), Point(1 / 3.0), Color);
                part2 = new CubicEdge(Point(1 / 3.0),
                    Vector2D.Lerp(Vector2D.Lerp(Vector2D.Lerp(p[P0], p[P1], 1 / 3.0), Vector2D.Lerp(p[P1], p[CP1], 1 / 3.0), 1 / 3.0), Vector2D.Lerp(Vector2D.Lerp(p[P1], p[CP1], 1 / 3.0), Vector2D.Lerp(p[CP1], p[CP2], 1 / 3.0), 1 / 3.0), 2 / 3.0),
                    Vector2D.Lerp(Vector2D.Lerp(Vector2D.Lerp(p[P0], p[P1], 2 / 3.0), Vector2D.Lerp(p[P1], p[CP1], 2 / 3.0), 2 / 3.0), Vector2D.Lerp(Vector2D.Lerp(p[P1], p[CP1], 2 / 3.0), Vector2D.Lerp(p[CP1], p[CP2], 2 / 3.0), 2 / 3.0), 1 / 3.0),
                    Point(2 / 3.0), Color);
                part3 = new CubicEdge(Point(2 / 3.0), Vector2D.Lerp(Vector2D.Lerp(p[P1], p[CP1], 2 / 3.0), Vector2D.Lerp(p[CP1], p[CP2], 2 / 3.0), 2 / 3.0), p[CP1] == p[CP2] ? p[CP2] : Vector2D.Lerp(p[CP1], p[CP2], 2 / 3.0), p[CP2], Color);
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
                    int solutions = SignedDistanceSolver.SolveCubic(t, ass.Y, 3 * br.Y, 3 * ab.Y, p[0].Y - y);

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

                Vector2D epDir = GetDirection(0);
                double minDistance = MathHelperDP.NonZeroSign(Vector2D.Cross(epDir, qa)) * qa.Length(); // distance from A
                param = -Vector2D.Dot(qa, epDir) / Vector2D.Dot(epDir, epDir);
                {
                    epDir = GetDirection(1);
                    double distance = (p[3] - origin).Length(); // distance from B
                    if (distance < Math.Abs(minDistance))
                    {
                        minDistance = MathHelperDP.NonZeroSign(Vector2D.Cross(epDir, p[3] - origin)) * distance;
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
                            minDistance = MathHelperDP.NonZeroSign(Vector2D.Cross(d1, qe)) * distance;
                            param = t;
                        }
                    }
                }

                if (param >= 0 && param <= 1)
                    return new SignedDistance(minDistance, 0);
                if (param < .5)
                    return new SignedDistance(minDistance, Math.Abs(Vector2D.Dot(GetDirection(0).GetNormalized(), qa.GetNormalized())));
                else
                    return new SignedDistance(minDistance, Math.Abs(Vector2D.Dot(GetDirection(1).GetNormalized(), (p[3] - origin).GetNormalized())));
            }
        }
    }
}
